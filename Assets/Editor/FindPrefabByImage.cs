using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;

public class FindPrefabByImage : EditorWindow {
        private static string assetPath;
        private static List<string> prefabPaths = new List<string> ();
        private bool initialized = false;
        private SearchField searchField;
        private string searchStr;
        Rect toolbarRect {
            get { return new Rect (20f, 10f, position.width - 40f, 20f); }
        }

        Rect searchResultViewRect {
            get { return new Rect (20f, 40f, (position.width - 40) / 2, position.height - 40f); }
        }

        Rect selectPrefabHierarchyViewRect {
            get { return new Rect (position.width / 2, 40, (position.width - 40) / 2, position.height - 40f); }
        }

        Vector2 searchResultScrollViewPos = Vector2.zero;

        [SerializeField]
        TreeViewState treeViewState;
        PrefabTreeView prefabTreeView;

        private GameObject curSelectPrefab;

        void OnEnable () {
            if (null == treeViewState)
                treeViewState = new TreeViewState ();
        }

        [MenuItem ("CommonTools/图片反查预制体引用工具")]
        public static FindPrefabByImage GetWindow () {
            prefabPaths.Clear ();
            assetPath = Application.dataPath;
            GetFiles (new DirectoryInfo (assetPath), "*.prefab", ref prefabPaths);
            var window = GetWindow<FindPrefabByImage> ();
            window.titleContent = new GUIContent ("FindPrefab");
            window.Focus ();
            window.Repaint ();
            return window;
        }

        public static void GetFiles (DirectoryInfo directory, string pattern, ref List<string> fileList) {
            if (directory != null && directory.Exists && !string.IsNullOrEmpty (pattern)) {
                try {
                    foreach (FileInfo info in directory.GetFiles (pattern)) {
                        string path = info.FullName.ToString ();
                        fileList.Add (path.Substring (path.IndexOf ("Assets")));
                    }
                } catch (System.Exception) {

                    throw;
                }
                foreach (DirectoryInfo info in directory.GetDirectories ()) {
                    GetFiles (info, pattern, ref fileList);
                }
            }
        }

        /// <summary>
        /// 检测预制体是否包含Image组件
        /// </summary>
        /// <param name="prefab">检测的预制体</param>
        /// <param name="imageName">Image名称，可为空</param>
        /// <returns>如果Image名称为空且预制体包含Image组件，或者Image不为空预制体包含该指定Image，则返回true，否则返回false</returns>
        public static bool CheckPrefabHasImage (GameObject prefab, string imageName = null) {
            if (!string.IsNullOrEmpty (imageName))
                imageName = imageName.ToLower ();
            if (null == prefab) return false;
            Image[] images = prefab.GetComponentsInChildren<Image> (true);
            if (null != images && images.Length > 0) {
                if (string.IsNullOrEmpty (imageName)) {
                    return true;
                } else {
                    for (int i = 0; i < images.Length; i++) {
                        if (null != images[i] && images[i].sprite != null && images[i].sprite.name.ToLower() == imageName) {
                            return true;
                        }
                    }
                }
            }
            RawImage[] rawImages = prefab.GetComponentsInChildren<RawImage> (true);
            if (null != rawImages && rawImages.Length > 0) {
                if (string.IsNullOrEmpty (imageName)) {
                    return true;
                } else {
                    for (int i = 0; i < rawImages.Length; i++) {
                        if (null != rawImages[i] && null != rawImages[i].texture && rawImages[i].texture.name.ToLower() == imageName) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void InitIfNeeded () {
            if (!initialized) {
                if (null == searchField)
                    searchField = new SearchField ();
                initialized = true;
            }
        }

        void OnGUI () {
            InitIfNeeded ();
            DoSearchField ();

            DoSearchResultView ();
            DoSelectPrefabHierarchyVieww ();
        }

        private void DoSearchField () {
            searchStr = searchField.OnGUI (toolbarRect, searchStr);
        }

        private void DoSearchResultView () {
            GUILayout.BeginArea (searchResultViewRect);
            searchResultScrollViewPos = GUILayout.BeginScrollView (searchResultScrollViewPos);
            
            for (int i = 0; i < prefabPaths.Count; i++) {
                 GameObject gameObj = AssetDatabase.LoadAssetAtPath<GameObject> (@prefabPaths[i]);
                if (gameObj != null)
                {
                    bool result = CheckPrefabHasImage (gameObj,searchStr);
                    if (result)
                    {
                        if (GUILayout.Button (prefabPaths[i]))
                        {
                            curSelectPrefab = gameObj;
                        }
                    }
                }
            }
            
            GUILayout.EndScrollView ();
            GUILayout.EndArea ();
        }

    private void DoSelectPrefabHierarchyVieww () {
        GUILayout.BeginArea (selectPrefabHierarchyViewRect);
        if (null != curSelectPrefab) {
            Rect rect = GUILayoutUtility.GetRect (0, 100000, 0, 100000);
            prefabTreeView = new PrefabTreeView (treeViewState, curSelectPrefab);

            prefabTreeView.OnGUI (rect);
            prefabTreeView.ExpandAll ();
        } else {

        }
        GUILayout.EndArea ();
    }
}