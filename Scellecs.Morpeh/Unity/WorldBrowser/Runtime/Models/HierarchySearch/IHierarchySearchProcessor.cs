﻿#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IHierarchySearchProcessor {
        public HierarchySearchModel GetModel();
        public void SetSearchString(string value);
        public void SetComponentIncluded(int id, bool included, QueryParam param);
    }
}
#endif