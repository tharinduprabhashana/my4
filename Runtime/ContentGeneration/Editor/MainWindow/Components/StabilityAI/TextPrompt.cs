using System;
using System.Collections.Generic;
using ContentGeneration.Models.Stability;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.StabilityAI
{
    public class TextPrompt : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TextPrompt, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        PromptInput promptInput => this.Q<PromptInput>("promptInput");
        FloatField weight => this.Q<FloatField>("weight");
        Button removePrompt => this.Q<Button>("removePrompt");
        Button improvePrompt => this.Q<Button>("improvePrompt");

        public Prompt prompt =>
            new()
            {
                Text = promptInput.value,
                Weight = weight.value,
            };

        public TextPrompt() : this(null, null)
        {
            
        }

        public TextPrompt(Action<TextPrompt> onRemove, Action onChanged)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/ContentGeneration/Editor/MainWindow/Components/StabilityAI/TextPrompt.uxml");
            asset.CloneTree(this);

            removePrompt.clicked += () => onRemove(this);
            promptInput.OnChanged += _ => onChanged();
            weight.RegisterValueChangedCallback(v =>
            {
                weight.value = Mathf.Min(1, Mathf.Max(-1, v.newValue));
                onChanged();
            });
            improvePrompt.clicked += () =>
            {
                if (string.IsNullOrEmpty(promptInput.value))
                    return;
                
                if(!improvePrompt.enabledSelf)
                    return;

                improvePrompt.SetEnabled(false);
                
                ContentGenerationApi.Instance.ImprovePrompt(promptInput.value, "stability").ContinueInMainThreadWith(
                    t =>
                    {
                        improvePrompt.SetEnabled(true);
                        if (t.IsFaulted)
                        {
                            Debug.LogException(t.Exception!.InnerException!);
                            return;
                        }

                        promptInput.value = t.Result;
                    });
            };
        }
    }
}