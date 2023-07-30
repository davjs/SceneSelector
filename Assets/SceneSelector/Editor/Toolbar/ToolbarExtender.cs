using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// This code has been refined from: https://github.com/marijnz/unity-toolbar-extender

namespace SceneSelector.Editor.Toolbar
{
	[InitializeOnLoad]
	public static class ToolbarExtender
	{
		private static readonly Type ToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

		private static ScriptableObject _currentToolbar;

		public static readonly List<Action> LeftToolbarGUI = new();
		
		static ToolbarExtender()
		{
			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
		}

		private static void OnUpdate()
		{
			if (_currentToolbar == null)
			{
				_currentToolbar = Resources.FindObjectsOfTypeAll(ToolbarType).FirstOrDefault() as ScriptableObject;
				if (_currentToolbar != null)
				{ 
					var root = _currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
					if (root != null) {
						var rawRoot = root.GetValue(_currentToolbar);
						var mRoot = rawRoot as VisualElement;
						RegisterCallback("ToolbarZoneLeftAlign", LeftToolbarGUI, mRoot);
					}
				}
			}
		}

		private static void RegisterCallback(string query, List<Action> cb, VisualElement mRoot) {
			var toolbarZone = mRoot.Q(query);
			var parent = new VisualElement {
				style = {
					flexGrow = 1,
					flexDirection = FlexDirection.Row,
				}
			};
			var container = new IMGUIContainer();
			container.style.flexGrow = 1;
			container.onGUIHandler += () => { 
				GUILayout.BeginHorizontal();
				foreach (var handler in cb)
				{
					handler();
				}
				GUILayout.EndHorizontal();
			}; 
			parent.Add(container);
			toolbarZone.Add(parent);
		}
	}
}