using BepInEx;
using GorillaExtensions;
using GorillaNetworking;
using GorillaNetworking.Store;
using HarmonyLib;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Overstocked;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    private static Plugin _instance = null!;
    private static Transform? _mirrorSofa;
    private DynamicCosmeticStand? _defaultStand;
    private CosmeticsController.CosmeticCategory _currentSelectedCategory = (CosmeticsController.CosmeticCategory)1;
    private readonly List<DynamicCosmeticStand> _stands = new();
    private readonly Dictionary<CosmeticsController.CosmeticCategory, HashSet<CosmeticsController.CosmeticItem>> _allItemsWithPrice = new();
    private readonly Dictionary<CosmeticsController.CosmeticCategory, int> _categoryPage = new();
    private Util? _utility;

    private int TotalCosmetics => _allItemsWithPrice.Sum(items => items.Value.Count);

    private void Awake()
    {
        _instance = this;
        new Harmony(PluginInfo.Guid).PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
        GorillaTagger.OnPlayerSpawned(() =>
        {
            CosmeticsController.instance.V2_OnGetCosmeticsPlayFabCatalogData_PostSuccess += OnCosmeticsLoaded;
            _defaultStand = Resources.FindObjectsOfTypeAll<DynamicCosmeticStand>()
                .First(x => x.name == "DynamicCosmeticStand_CustomMap");
            _utility = new Util(_defaultStand.AddToCartButton.transform.parent.gameObject);
        });
    }

    private static void SetItemForStand(DynamicCosmeticStand stand, CosmeticsController.CosmeticItem item)
    {
        stand.AddToCartButton.isOn = false;
        stand.AddToCartButton.UpdateColor();

        if (CosmeticsController.instance.currentCart.Contains(item))
        {
            stand.AddToCartButton.isOn = true;
            stand.AddToCartButton.UpdateColor();
        }

        stand.thisCosmeticName = item.itemName;
        stand.StandName = item.itemName;
        StoreController.instance.AddStandToPlayfabIDDictionary(stand);
        stand.InitializeCosmetic();

        switch ((int)item.itemCategory)
        {
            case 1:
            case 3:
            case 4:
                stand.SetStandType((HeadModel_CosmeticStand.BustType)1);
                break;
            case 2:
            case 5:
            case 7:
            case 8:
            case 9:
                stand.SetStandType((HeadModel_CosmeticStand.BustType)2);
                break;
            case 6:
                stand.SetStandType((HeadModel_CosmeticStand.BustType)4);
                break;
            case 10:
            case 13:
                stand.SetStandType((HeadModel_CosmeticStand.BustType)3);
                break;
            case 11:
                stand.SetStandType((HeadModel_CosmeticStand.BustType)7);
                break;
            default:
                stand.SetStandType((HeadModel_CosmeticStand.BustType)0);
                break;
        }
    }

    private void SetStandsPageForCategory(CosmeticsController.CosmeticCategory category, int page)
    {
        _categoryPage[category] = page;

        if (!_allItemsWithPrice.TryGetValue(category, out var source))
            return;

        var list = source.Skip(page * _stands.Count).Take(_stands.Count).ToList();

        for (int i = 0; i < _stands.Count; i++)
        {
            SetItemForStand(_stands[i], list.Count <= i
                ? CosmeticsController.instance.nullItem
                : list[i]);
        }
    }

    private IEnumerator SetStandsPageForCategoryNextFrame(CosmeticsController.CosmeticCategory category, int page)
    {
        yield return new WaitForEndOfFrame();
        SetStandsPageForCategory(category, page);
    }

    private int GetPagesForCategory(CosmeticsController.CosmeticCategory category)
    {
        if (!_allItemsWithPrice.TryGetValue(category, out var items) || _stands.Count == 0)
            return 0;

        return (items.Count + _stands.Count - 1) / _stands.Count;
    }

    private void OnCosmeticsLoaded()
    {
        var cosmeticItems = CosmeticsController.instance.allCosmetics
            .Where(x => x.canTryOn && x.itemCategory != null && !CosmeticsController.instance.unlockedCosmetics.Contains(x));

        var bundledItems = new HashSet<CosmeticsController.CosmeticItem>();
        foreach (var bundle in CosmeticsController.instance.allCosmetics.Where(x => x.itemCategory == (CosmeticsController.CosmeticCategory)13))
        {
            if (bundle.bundledItems?.Length > 0)
                foreach (var id in bundle.bundledItems)
                    bundledItems.Add(CosmeticsController.instance.GetItemFromDict(id));
        }

        foreach (var item in cosmeticItems)
        {
            if (_allItemsWithPrice.TryGetValue(item.itemCategory, out var set))
            {
                if (item.itemCategory == (CosmeticsController.CosmeticCategory)13 || !bundledItems.Contains(item))
                    set.Add(item);
            }
            else
            {
                _allItemsWithPrice.Add(item.itemCategory, new HashSet<CosmeticsController.CosmeticItem> { item });
                _categoryPage.Add(item.itemCategory, 0);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "City")
            return;

        Transform mirrorSofa;
        Transform tryOnConsole;

        if (!scene.TryFindByPath("City_Pretty/CosmeticsRoomAnchor/nicegorillastore_prefab/DressingRoom_Furniture_Prefab/mirror sofa/", out mirrorSofa, false) ||
            !scene.TryFindByPath("City_Pretty/CosmeticsRoomAnchor/nicegorillastore_prefab/DressingRoom_Mirrors_Prefab/TryOnConsole/", out tryOnConsole, false))
        {
            Logger.LogError("Could not find mirror sofa or TryOnConsole");
            return;
        }

        _mirrorSofa = mirrorSofa;
        LoadHeads(_mirrorSofa);
        SetUpCategoryButtons(_mirrorSofa);
        SetUpPageButtons(_mirrorSofa);
        SetUpText(_mirrorSofa);
        SetUpClearCartButton(tryOnConsole);
    }

    private void LoadHeads(Transform parent)
    {
        if (_defaultStand == null)
            return;

        Vector3 pos = new(-1.2f, -0.2f, 0.45f);

        for (int i = 0; i < 3; i++)
        {
            pos.x += 0.6f;
            Transform t = Object.Instantiate(_defaultStand, parent, false).transform;
            t.localPosition = pos;
            t.localRotation = Quaternion.Euler(270f, 180f, 0f);
            _stands.Add(t.GetComponent<DynamicCosmeticStand>());
        }

        StartCoroutine(SetStandsPageForCategoryNextFrame(_currentSelectedCategory, 0));
    }

    private void SetUpCategoryButtons(Transform parent)
    {
        if (_utility == null)
        {
            Logger.LogError("Utility class is null most likely failed to get headstand model");
            return;
        }

        GameObject container = new("CosmeticSetButtons");
        container.transform.SetParent(parent, false);
        container.transform.localPosition = new Vector3(0f, 0.1f, 0.1f);
        container.transform.localRotation = Quaternion.Euler(300f, 0f, 180f);
        container.transform.localScale = Vector3.one * 0.7f;

        GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(0.2f, 0.13f);
        grid.childAlignment = (TextAnchor)4;
        grid.startCorner = GridLayoutGroup.Corner.LowerLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        foreach (var set in _allItemsWithPrice.OrderByDescending(kvp => kvp.Value.Count).Reverse())
        {
            var captured = set;
            string text = $"{captured.Value.Count}\n{captured.Key.ToString().ToUpper()}";
            _utility.CreateButton(container.transform, new Vector3(0f, 0f, 0.042f), Vector3.one * 0.004f, text, () =>
            {
                _currentSelectedCategory = captured.Key;
                StartCoroutine(SetStandsPageForCategoryNextFrame(_currentSelectedCategory, _categoryPage[captured.Key]));
            }).AddComponent<RectTransform>();
        }
    }

    private void SetUpPageButtons(Transform parent)
    {
        if (_utility == null)
        {
            Logger.LogError("Utility class is null most likely failed to get headstand model");
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            bool isLeft = i == 1;
            string text = isLeft ? "<" : ">";
            GameObject button = _utility.CreateButton(parent, new Vector3(0f, 0f, 0.042f), Vector3.one * 0.004f, text, () =>
            {
                int pages = GetPagesForCategory(_currentSelectedCategory);
                int page = (_categoryPage[_currentSelectedCategory] + (isLeft ? -1 : 1) + pages) % pages;
                _categoryPage[_currentSelectedCategory] = page;
                StartCoroutine(SetStandsPageForCategoryNextFrame(_currentSelectedCategory, page));
            });

            button.name = "Page Selector " + text;
            button.transform.localPosition = isLeft ? new Vector3(-0.3f, -0.02f, 0.3f) : new Vector3(0.3f, -0.02f, 0.3f);
            button.transform.localRotation = Quaternion.Euler(315.2f, 0f, 90f);
            button.transform.GetChild(1).localRotation = Quaternion.Euler(0f, 0f, 270f);
        }
    }

    private void SetUpText(Transform parent)
    {
        if (_defaultStand == null)
        {
            Logger.LogError("Failed to get defaultStand");
            return;
        }

        GameObject go = new("Text");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, 0.753f, 0f);
        go.transform.localScale = Vector3.one * 0.1f;
        go.transform.localRotation = Quaternion.Euler(45f, 180f, 180f);

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.font = _defaultStand.AddToCartButton.transform.parent.GetChild(1).GetComponent<TextMeshPro>().font;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 10f;
        tmp.text = $"{TotalCosmetics} COSMETICS";
    }

    private void SetUpClearCartButton(Transform parent)
    {
        GameObject clone = Object.Instantiate(parent.GetChild(2).gameObject, parent, false);
        clone.transform.localPosition = new Vector3(0.1895f, 0.2657f, -0.2043f);

        PlayerPrefFlagButton oldButton = clone.GetComponent<PlayerPrefFlagButton>();
        GorillaPressableButton newButton = clone.AddComponent<GorillaPressableButton>();

        GameObject textObj = Object.Instantiate(((GorillaPressableButton)oldButton).myTmpText.gameObject, clone.transform, false);
        textObj.transform.localPosition = new Vector3(19.398f, -0.6059f, -26.9f);
        textObj.transform.localScale = ((GorillaPressableButton)oldButton).myTmpText.transform.localScale;
        textObj.transform.localRotation = ((GorillaPressableButton)oldButton).myTmpText.transform.localRotation;

        newButton.myTmpText = textObj.GetComponent<TextMeshPro>();
        newButton.buttonRenderer = ((GorillaPressableButton)oldButton).buttonRenderer;
        newButton.unpressedMaterial = ((GorillaPressableButton)oldButton).unpressedMaterial;
        newButton.isOn = false;
        newButton.offText = "CLEAR";
        newButton.UpdateColor();
        newButton.onPressed += (_, _) =>
        {
            CosmeticsController.instance.ClearCheckoutAndCart(false);
            StartCoroutine(SetStandsPageForCategoryNextFrame(_currentSelectedCategory, _categoryPage[_currentSelectedCategory]));
        };

        Object.Destroy(oldButton);
    }
}