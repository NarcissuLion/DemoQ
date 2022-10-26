using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Framework.Lua;

namespace Framework.LuaMVC.Editor
{
    public class LuaViewScriptGenerator
    {
        private static string LUA_VIEW_DIR = GlobalDefine.LUA_ROOT_DIR + "MVC/View/";

        public static void Generate(LuaViewFacade facade)
        {
            GenerateView(facade);
        }

        private static bool ValidateFacade(LuaViewFacade facade)
        {
            HashSet<string> keywords = new HashSet<string>(2) { "gameObject", "transform" };
            foreach (LuaViewComponent comp in facade.comps)
            {
                if (keywords.Contains(comp.name))
                {
                    EditorUtility.DisplayDialog("Error!", comp.name + " is a keyword in generated code. can't use it as the name of component.", "OK");
                    return false;
                }
                if (!Regex.IsMatch(comp.name, "^[_a-zA-Z]+[_a-zA-Z0-9]*$"))
                {
                    EditorUtility.DisplayDialog("Error!", comp.name + " does not conform to variable naming conventions.", "OK");
                    return false;
                }
            }

            return true;
        }

        private static Stack<LuaViewFacade> _subViewStack = new Stack<LuaViewFacade>();
        private static HashSet<string> _subTblNameSet = new HashSet<string>();
        private static void GenerateView(LuaViewFacade facade)
        {
            if (!ValidateFacade(facade)) return;
            
            _subViewStack.Clear();
            _subTblNameSet.Clear();
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.Clear();
            strBuilder.AppendLine("-- 本脚本为自动生成，不要手动修改，以免被覆盖");

            #region declare table
            strBuilder.AppendFormat("local {0} = ", facade.viewName).AppendLine("{}");
            // strBuilder.AppendLine("local ViewBase = require 'Framework.MVC.ViewBase'");
            strBuilder.AppendFormat("{0}.__index = {0}", facade.viewName).AppendLine();
            // strBuilder.AppendFormat("setmetatable({0}, ViewBase)", facade.viewName).AppendLine();
            strBuilder.AppendFormat("{0}.__PREFAB_ASSET = '{1}'", facade.viewName, AssetDatabase.GetAssetPath(facade)).AppendLine();
            strBuilder.AppendLine().AppendLine("local Notifier = require 'Framework.Notifier'");
            #endregion

            #region func Create
            strBuilder.AppendFormat("function {0}.Create(facade)", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendLine("local copy = {}");
            strBuilder.Append("\t").AppendFormat("setmetatable(copy, {0})", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendLine("copy:Init(facade)");
            strBuilder.Append("\t").AppendLine("return copy");
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            GenerateViewComps(facade, out StringBuilder compsInitBuider, out StringBuilder compsRenderBuilder);

            #region func Init
            strBuilder.AppendFormat("function {0}:Init(facade)", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendFormat("assert(facade ~= nil, 'Error! {0} facade is nil')", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendLine("facade:SetComps(self)");
            strBuilder.Append("\t").AppendLine("self.gameObject = facade.gameObject");
            strBuilder.Append("\t").AppendLine("self.transform = facade.transform");
            strBuilder.Append(compsInitBuider);
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            #region func Open
            strBuilder.AppendFormat("function {0}:Open(viewModel)", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendFormat("assert(self.gameObject ~= nil, 'Error! {0} has been disposed.')", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendLine("Notifier.Dispatch('__OPEN_VIEW_BEFORE', self)");
            strBuilder.Append("\t").AppendLine("self.gameObject:SetActive(true)");
            strBuilder.Append("\t").AppendLine("if viewModel ~= nil then self:Render(viewModel) end");
            strBuilder.Append("\t").AppendLine("Notifier.Dispatch('__OPEN_VIEW_AFTER', self)");
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            #region func Close
            strBuilder.AppendFormat("function {0}:Close()", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendFormat("assert(self.gameObject ~= nil, 'Error! {0} has been disposed.')", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendLine("Notifier.Dispatch('__CLOSE_VIEW_BEFORE', self)");
            strBuilder.Append("\t").AppendLine("self.gameObject:SetActive(false)");
            strBuilder.Append("\t").AppendLine("Notifier.Dispatch('__CLOSE_VIEW_AFTER', self)");
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            #region func Dispose
            strBuilder.AppendFormat("function {0}:Dispose()", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendFormat("assert(self.gameObject ~= nil, 'Error! {0} has been disposed.')", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendLine("GameObject.Destroy(self.gameObject)");
            strBuilder.Append("\t").AppendLine("self.gameObject = nil");
            strBuilder.Append("\t").AppendLine("self.transform = nil");
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            #region func Render
            strBuilder.AppendFormat("function {0}:Render(viewModel)", facade.viewName).AppendLine();
            strBuilder.Append("\t").AppendFormat("assert(viewModel ~= nil, 'Error! {0} view model is nil')", facade.viewName).AppendLine();
            strBuilder.Append(compsRenderBuilder);
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            #region sub views
            while (_subViewStack.Count > 0)
            {
                LuaViewFacade subFacade = _subViewStack.Pop();
                if (!GenerateSubView(subFacade, ref strBuilder)) return;
            }
            #endregion

            #region final return
            strBuilder.AppendFormat("return {0}", facade.viewName);
            #endregion
            
            string path = LUA_VIEW_DIR + facade.viewName + ".lua";
            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog("Notice", path + "\n Overwrite the exist file?", "Yes", "No")) File.Delete(path);
                else return;
            }
            File.WriteAllText(path, strBuilder.ToString());
        }

        private static void GenerateViewComps(LuaViewFacade facade, out StringBuilder initBuilder, out StringBuilder renderBuilder)
        {
            initBuilder = new StringBuilder();
            renderBuilder = new StringBuilder();
            foreach (LuaViewComponent comp in facade.comps)
            {
                switch (comp.type)
                {
                    case "Object":
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.actived ~= nil then self.{0}:SetActive(viewModel.{0}.actived) end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.position ~= nil then self.{0}.transform.localPosition = viewModel.{0}.position end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.rotation ~= nil then self.{0}.transform.localRotation = viewModel.{0}.rotation end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.euler ~= nil then self.{0}.transform.localEulerAngles = viewModel.{0}.euler end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.scale ~= nil then self.{0}.transform.localScale = viewModel.{0}.scale end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.parent ~= nil then self.{0}.transform.parent = viewModel.{0}.parent end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "LuaViewFacade":
                        initBuilder.Append("\t").AppendFormat("self.{0} = {1}.{2}.Create(self.view_xxx)", comp.name, facade.viewName, ((LuaViewFacade)comp.target).viewName).AppendLine();
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("self.{0}:Render(viewModel.{0})", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        _subViewStack.Push((LuaViewFacade)comp.target);
                        break;
                    case "Image":
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.color ~= nil then self.{0}.color = viewModel.{0}.color end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.sprite ~= nil then self.{0}.sprite = viewModel.{0}.sprite end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "RawImage":
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.color ~= nil then self.{0}.color = viewModel.{0}.color end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.texture ~= nil then self.{0}.texture = viewModel.{0}.texture end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "Text":
                    case "TextMeshProUGUI":
                        // if (!string.IsNullOrEmpty(comp.paramStr)) initBuilder.Append("\t").AppendFormat("self.{0}.text = 'Static Label' -- lookup language table later.", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.text ~= nil then self.{0}.text = viewModel.{0}.text end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.color ~= nil then self.{0}.color = viewModel.{0}.color end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "InputField":
                    case "TMP_InputField":
                        initBuilder.Append("\t").AppendFormat("self.{0}_OnValueChanged = nil", comp.name).AppendLine();
                        initBuilder.Append("\t").AppendFormat("self.{0}.onValueChanged:AddListener(function(text) if self.{0}_OnValueChanged ~= nil then self.{0}_OnValueChanged(text) end end)", comp.name).AppendLine();
                        initBuilder.Append("\t").AppendFormat("self.{0}_OnSubmit = nil", comp.name).AppendLine();
                        initBuilder.Append("\t").AppendFormat("self.{0}.onSubmit:AddListener(function(text) if self.{0}_OnSubmit ~= nil then self.{0}_OnSubmit(text) end end)", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.interactable ~= nil then self.{0}.interactable = viewModel.{0}.interactable end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.text ~= nil then self.{0}.text = viewModel.{0}.text end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "Toggle":
                        initBuilder.Append("\t").AppendFormat("self.{0}_OnValueChanged = nil", comp.name).AppendLine();
                        initBuilder.Append("\t").AppendFormat("self.{0}.onValueChanged:AddListener(function(isOn) if self.{0}_OnValueChanged ~= nil then self.{0}_OnValueChanged(isOn) end end)", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.interactable ~= nil then self.{0}.interactable = viewModel.{0}.interactable end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.isOn ~= nil then self.{0}.isOn = viewModel.{0}.isOn end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "ToggleGroup":
                        break;
                    case "Button":
                        initBuilder.Append("\t").AppendFormat("self.{0}_OnClick = nil", comp.name).AppendLine();
                        initBuilder.Append("\t").AppendFormat("self.{0}.onClick:AddListener(function() if self.{0}_OnClick ~= nil then self.{0}_OnClick() end end)", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.interactable ~= nil then self.{0}.interactable = viewModel.{0}.interactable end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "Slider":
                        initBuilder.Append("\t").AppendFormat("self.{0}_OnValueChanged = nil", comp.name).AppendLine();
                        initBuilder.Append("\t").AppendFormat("self.{0}.onValueChanged:AddListener(function(value) if self.{0}_OnValueChanged ~= nil then self.{0}_OnValueChanged(value) end end)", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.interactable ~= nil then self.{0}.interactable = viewModel.{0}.interactable end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.value ~= nil then self.{0}.value = viewModel.{0}.value end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "Scrollbar":
                        initBuilder.Append("\t").AppendFormat("self.{0}_OnValueChanged = nil", comp.name).AppendLine();
                        initBuilder.Append("\t").AppendFormat("self.{0}.onValueChanged:AddListener(function(value) if self.{0}_OnValueChanged ~= nil then self.{0}_OnValueChanged(value) end end)", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.interactable ~= nil then self.{0}.interactable = viewModel.{0}.interactable end", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.value ~= nil then self.{0}.value = viewModel.{0}.value end", comp.name).AppendLine();
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    case "ScrollRect":
                        renderBuilder.Append("\t").AppendFormat("if viewModel.{0} ~= nil then", comp.name).AppendLine();
                        renderBuilder.Append("\t\t").AppendFormat("if viewModel.{0}.enabled ~= nil then self.{0}.enabled = viewModel.{0}.enabled end", comp.name).AppendLine();
                        // 滑动列表项 内部池
                        renderBuilder.Append("\t").AppendLine("end");
                        break;
                    default:
                        Debug.LogWarning("ignored unsupport component type: " + comp.type);
                        break;
                }
            }
        }

        private static bool GenerateSubView(LuaViewFacade subFacade, ref StringBuilder strBuilder)
        {
            if (!ValidateFacade(subFacade)) return false;

            string subViewTblName = GetFacadeTblName(subFacade);
            if (_subTblNameSet.Contains(subViewTblName))
            {
                EditorUtility.DisplayDialog("Error!", "detected same name of sub facade: " + subViewTblName, "OK");
                return false;
            }
            _subTblNameSet.Add(subViewTblName);

            strBuilder.Append("-- Auto Generate: ").AppendLine(subViewTblName);
            #region declare table
            strBuilder.AppendFormat("{0} = ", subViewTblName).AppendLine("{}");
            strBuilder.AppendFormat("{0}.__index = {0}", subViewTblName).AppendLine();
            strBuilder.AppendLine();
            #endregion

            #region func Create
            strBuilder.AppendFormat("function {0}.Create(facade)", subViewTblName).AppendLine();
            strBuilder.Append("\t").AppendLine("local copy = {}");
            strBuilder.Append("\t").AppendFormat("setmetatable(copy, {0})", subViewTblName).AppendLine();
            strBuilder.Append("\t").AppendLine("copy:Init(facade)");
            strBuilder.Append("\t").AppendLine("return copy");
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            GenerateViewComps(subFacade, out StringBuilder compsInitBuilder, out StringBuilder compsRenderBuilder);

            #region func Init
            strBuilder.AppendFormat("function {0}:Init(facade)", subViewTblName).AppendLine();
            strBuilder.Append("\t").AppendFormat("assert(facade ~= nil, 'Error! {0} facade is nil')", subViewTblName).AppendLine();
            strBuilder.Append("\t").AppendLine("facade:SetComps(self)");
            strBuilder.Append(compsInitBuilder);
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            #region func Render
            strBuilder.AppendFormat("function {0}:Render(viewModel)", subViewTblName).AppendLine();
            strBuilder.Append("\t").AppendFormat("assert(viewModel ~= nil, 'Error! {0} view model is nil')", subViewTblName).AppendLine();
            strBuilder.Append(compsRenderBuilder);
            strBuilder.AppendLine("end");
            strBuilder.AppendLine();
            #endregion

            return true;
        }

        private static string GetFacadeTblName(LuaViewFacade facade)
        {
            Stack<string> stack = new Stack<string>();
            stack.Push(facade.viewName);

            Transform node = facade.transform.parent;
            while (node != null)
            {
                LuaViewFacade pFacade = node.GetComponent<LuaViewFacade>();
                if (pFacade != null) stack.Push(pFacade.viewName);
                node = node.parent;
            }
            return string.Join('.', stack);
        }
    }
}