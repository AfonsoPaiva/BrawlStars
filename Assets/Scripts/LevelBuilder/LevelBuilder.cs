using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.WSA;

namespace GridEditor
{
    public class LevelBuilder : EditorWindow
    {
        [SerializeField] private string tilesFolder;
        [SerializeField] private string propsFolder;
        [SerializeField] private int dimensionX;
        [SerializeField] private int dimensionY;

        public VisualTreeAsset editorLayoutUXML;

        [MenuItem("Level builder/Open builder")]
        public static void ShowWindow() {
            LevelBuilder window = (LevelBuilder)GetWindow(typeof(LevelBuilder));
            window.Show();
            window.lvTiles.selectedIndex = 0;
            window.lvProps.selectedIndex = 0;
        }

        protected void OnDisable() {
            tilesFolder = txtTilesFolder.text;
            propsFolder = txtPropsFolder.text;
            dimensionX = sldrDimensionX.value;
            dimensionY = sldrDimensionY.value;
            var data = JsonUtility.ToJson(this, false);
            var data2 = JsonUtility.ToJson(txtTilesFolder, true);

            var LevelBuilderData = JsonUtility.ToJson(new LevelBuilderConfiguration(this));

            EditorPrefs.SetString("LevelBuilderEditorWindow", LevelBuilderData);
        }

        private void OnEnable() {

        }

        #region uitool binding
        TabView tabLevelBuilder;
        VisualElement veDefaultTile;
        Button btnCreate;
        public Vector3Field v3OriginLocation;
        public TextField txtOriginName;
        public SliderInt sldrDimensionX, sldrDimensionY;
        public Slider sldrTileSize;

        ListView lvTiles, lvProps;
        private Button btnSelectTilesFolder, btnSelectPropsFolder;
        [SerializeField] public TextField txtTilesFolder, txtPropsFolder;

        private void CreateGUI() {
            VisualElement windowRoot = new VisualElement();
            editorLayoutUXML.CloneTree(windowRoot);

            tabLevelBuilder = windowRoot.Q<TabView>("tabLevelBuilder");
            tabLevelBuilder.activeTabChanged += SetupSelectionChanged;

            btnCreate = windowRoot.Q<Button>("btnCreate");
            veDefaultTile = windowRoot.Q<VisualElement>("veDefaultTile");
            v3OriginLocation = windowRoot.Q<Vector3Field>("v3OriginLocation");
            txtOriginName = windowRoot.Q<TextField>("txtOriginName");
            sldrDimensionX = windowRoot.Q<SliderInt>("sldrDimensionX");
            sldrDimensionX.value = dimensionX;
            sldrDimensionY = windowRoot.Q<SliderInt>("sldrDimensionY");
            sldrDimensionY.value = dimensionY;
            sldrTileSize = windowRoot.Q<Slider>("sldrTileSize");

            lvTiles = windowRoot.Q<ListView>("lvTiles");
            lvTiles.makeItem = MakeItem;
            lvTiles.bindItem = BindItem;
            lvTiles.selectedIndicesChanged += PrefabSelectionChanged;
            btnSelectTilesFolder = windowRoot.Q<Button>("btnSelectTilesFolder");
            txtTilesFolder = windowRoot.Q<TextField>("txtTilesFolder");
            btnSelectTilesFolder.clicked += () =>
                {
                    string absolutePath = EditorUtility.OpenFolderPanel("Select Tiles Folder", "", "");
                    txtTilesFolder.value = absolutePath.Substring(absolutePath.IndexOf("Assets"));
                    LoadTiles();
                };

            lvProps = windowRoot.Q<ListView>("lvProps");
            lvProps.makeItem = MakeItem;
            lvProps.bindItem = BindItem;
            lvProps.selectedIndicesChanged += PrefabSelectionChanged;
            btnSelectPropsFolder = windowRoot.Q<Button>("btnSelectPropsFolder");
            txtPropsFolder = windowRoot.Q<TextField>("txtPropsFolder");
            btnSelectPropsFolder.clicked += () =>
                {
                    string absolutePath = EditorUtility.OpenFolderPanel("Select Props Folder", "", "");
                    txtPropsFolder.value = absolutePath.Substring(absolutePath.IndexOf("Assets"));
                    LoadProps();
                };

            btnCreate.clicked += CreateGrid;
            this.rootVisualElement.Add(windowRoot);

            var data = EditorPrefs.GetString("LevelBuilderEditorWindow", JsonUtility.ToJson(this, false));
            LevelBuilderConfiguration config = JsonUtility.FromJson<LevelBuilderConfiguration>(data);

            //JsonUtility.FromJsonOverwrite(data, this);
            config.ConfigureLevelBuilder(this);
            LoadTiles();
            LoadProps();
        }

        private void LoadTiles() {
            if (string.IsNullOrEmpty(txtTilesFolder.text)) return;
            lvTiles.itemsSource = GetAssets(GetAssetsPaths(txtTilesFolder.text));
            lvTiles.selectedIndex = 0;
            ShowDefaultTile();
        }
        private void LoadProps() {
            if (string.IsNullOrEmpty(txtPropsFolder.text)) return;
            lvProps.itemsSource = GetAssets(GetAssetsPaths(txtPropsFolder.text));
            lvProps.selectedIndex = 0;
        }

        #endregion

        private void SetupSelectionChanged(Tab tab1, Tab tab2) {
            lvProps.Rebuild();
            lvTiles.Rebuild();
        }

        private void PrefabSelectionChanged(IEnumerable<int> selection) {
            ShowDefaultTile();

            lvProps?.Rebuild();
            lvTiles?.Rebuild();
        }

        private void ShowDefaultTile() {
            if (lvTiles.selectedIndex == -1) return;
            Texture2D texture = GetPreviewTexture((GameObject)lvTiles.itemsSource[lvTiles.selectedIndex]);
            veDefaultTile.style.backgroundImage = new StyleBackground(texture);
        }

        private VisualElement MakeItem() {
            VisualElement img = new VisualElement();
            img.style.flexGrow = 1;
            img.style.height = 100;
            img.style.width = 100;

            return img;
        }
        private void BindItem(VisualElement img, int i) {
            ListView lv = img.parent.parent as ListView;
            Texture2D texture = GetPreviewTexture((GameObject)lv.itemsSource[i]);

            img.style.backgroundImage = new StyleBackground(texture);
            img.style.borderBottomWidth = img.style.borderTopWidth = img.style.borderLeftWidth = img.style.borderRightWidth = 2;

            int selectedIndex = (tabLevelBuilder.selectedTabIndex == 1) ? lvTiles.selectedIndex : lvProps.selectedIndex;
            StyleColor clr = (i == selectedIndex) ? Color.green : Color.black;
            img.style.borderBottomColor = img.style.borderTopColor = img.style.borderLeftColor = img.style.borderRightColor = clr;
        }

        private void CreateGrid() {
            GameObject grid = new GameObject();
            grid.transform.position = v3OriginLocation.value;
            grid.name = txtOriginName.text;
            BuildArea area = grid.AddComponent<BuildArea>();
            area.tileSize = sldrTileSize.value;
            area.dimension = new Vector2(sldrDimensionX.value, sldrDimensionY.value);
            area.BuildNodes();
            foreach (Node node in grid.GetComponentsInChildren<Node>())
            {
                AddPrefabToGrid(node, (GameObject)lvTiles.itemsSource[lvTiles.selectedIndex]);
            }
            EditorUtility.SetDirty(grid);
        }

        private Texture2D GetPreviewTexture(GameObject prefab) {
            return AssetPreview.GetAssetPreview(prefab);
        }
        private string[] GetAssetsPaths(String assetsFolder) {
            if (string.IsNullOrEmpty(assetsFolder)) return null;
            string[] guids = AssetDatabase
                .FindAssets("t:GameObject", new string[] { assetsFolder });
            //.FindAssets("t:GameObject", new string[] { $"Assets/scripts/LevelBuilder/{folder}" });
            return guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        }
        private GameObject[] GetAssets(string[] assetsPaths) {
            return assetsPaths.Select(path => (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject))).ToArray();
        }

        public void ClearGridNode(Node selectedNode) {
            //if we are placing tiles => remove tiles and props
            //if we are placing props => remove only props
            bool isPlacingTile = tabLevelBuilder.selectedTabIndex == 1;
            for (int i = selectedNode.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = selectedNode.transform.GetChild(i).gameObject;
                if (child.GetComponent<Prop>() != null && child.GetComponent<Prop>().GetType() == typeof(Prop))
                    DestroyImmediate(child);
                else if (tabLevelBuilder.selectedTabIndex == 1 && child.GetComponent<Tile>() != null)
                    DestroyImmediate(child);
            }
        }

        private void AddPrefabToGrid(Node selectedNode) {
            GameObject selectedPrefab = (GameObject)((tabLevelBuilder.selectedTabIndex == 1)
                    ? lvTiles.itemsSource[lvTiles.selectedIndex] : lvProps.itemsSource[lvProps.selectedIndex]);

            AddPrefabToGrid(selectedNode, selectedPrefab);
        }

        private void AddPrefabToGrid(Node selectedNode, GameObject selectedPrefab) {
            if (!HasPlacePrefabRequirements(selectedPrefab, selectedNode)) return;

            ClearGridNode(selectedNode);
            Vector3 objectPositionOffset =
                selectedNode.transform.position + new Vector3(0, selectedPrefab.GetComponent<Prop>().YOffset, 0);

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, selectedNode.transform);
            obj.transform.position = objectPositionOffset;

            obj.GetComponent<Prop>().buildArea = selectedNode.buildArea;
        }

        private bool HasPlacePrefabRequirements(GameObject selectedPrefab, Node selectedNode) {
            if (tabLevelBuilder.selectedTabIndex == 1) //tiles
            {
                selectedPrefab = (GameObject)lvTiles.itemsSource[lvTiles.selectedIndex];
                if (selectedPrefab.GetComponent<Tile>() == null)
                {
                    Debug.LogWarning("Tiles requires the Tile script");
                    return false;
                }
            }
            else if (tabLevelBuilder.selectedTabIndex == 2) //props
            {
                Tile tileInNode = selectedNode.GetComponentInChildren<Tile>();
                if (tileInNode == null || tileInNode.propability == Tile.Propability.NonPropable)
                {
                    Debug.LogWarning("Props can only be placed on propable tiles");
                    return false;
                }

                selectedPrefab = (GameObject)lvProps.itemsSource[lvProps.selectedIndex];
                if (selectedPrefab.GetComponent<Prop>() == null)
                {
                    Debug.LogWarning("Props requires the Prop script");
                    return false;
                }
            }
            return true;
        }

        private void OnSelectionChange() {
            if (Selection.activeGameObject != null)
            {
                Node selectedNode = Selection.activeGameObject.GetComponent<Node>();
                if (selectedNode != null)
                    AddPrefabToGrid(selectedNode);
            }
        }
    }
}

