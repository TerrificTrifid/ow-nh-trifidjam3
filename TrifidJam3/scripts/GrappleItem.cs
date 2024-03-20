using NewHorizons;
using NewHorizons.Components.Props;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace TrifidJam3
{
    public class GrappleItem : MonoBehaviour
    {
        private static NHItem _nhItem;

        private ScreenPrompt _activatePrompt;
        private ScreenPrompt _reelInPrompt;
        private ScreenPrompt _reelOutPrompt;

        private bool _activateFact;
        private bool _wallFact;

        private bool _active;

        private void Awake()
        {
            
        }

        private void Start()
        {
            _active = false;

            _nhItem = this.GetComponent<NHItem>();
            
            _activatePrompt = new ScreenPrompt(InputLibrary.lockOn, TranslationHandler.GetTranslation("Activate", TranslationHandler.TextType.UI) + "   <CMD>");
            _reelInPrompt = new ScreenPrompt(InputLibrary.thrustUp, TranslationHandler.GetTranslation("Reel In", TranslationHandler.TextType.UI) + "   <CMD>");
            _reelOutPrompt = new ScreenPrompt(InputLibrary.thrustDown, TranslationHandler.GetTranslation("Reel Out", TranslationHandler.TextType.UI) + "   <CMD>");
        }

        private void FixedUpdate()
        {
            var condition = DialogueConditionManager.SharedInstance.GetConditionState("GrappleActive");
            if (!_active && condition)
            {
                Locator.GetPromptManager().AddScreenPrompt(_activatePrompt, PromptPosition.UpperRight, true);
                Locator.GetPromptManager().AddScreenPrompt(_reelInPrompt, PromptPosition.UpperRight, true);
                Locator.GetPromptManager().AddScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight, true);

                _active = true;
            }
            if (_active && !condition)
            {
                Locator.GetPromptManager().RemoveScreenPrompt(_activatePrompt, PromptPosition.UpperRight);
                Locator.GetPromptManager().RemoveScreenPrompt(_reelInPrompt, PromptPosition.UpperRight);
                Locator.GetPromptManager().RemoveScreenPrompt(_reelOutPrompt, PromptPosition.UpperRight);

                _active = false;
            }

            if (_active)
            {
                if (OWInput.IsPressed(InputLibrary.lockOn, InputMode.Character))
                {
                    NHLogger.Log("activate");
                    if (!_activateFact)
                    {
                        Locator.GetShipLogManager().RevealFact("TJ3_Grapple_x1");
                        _activateFact = true;
                    }
                }
                if (OWInput.IsNewlyPressed(InputLibrary.thrustUp, InputMode.Character))
                {
                    NHLogger.Log("reel in");
                    if (!_wallFact)
                    {
                        Locator.GetShipLogManager().RevealFact("TJ3_Grapple_x2");
                        _wallFact = true;
                    }
                }
                if (OWInput.IsNewlyPressed(InputLibrary.thrustDown, InputMode.Character))
                {
                    NHLogger.Log("reel out");
                    if (!_wallFact)
                    {
                        Locator.GetShipLogManager().RevealFact("TJ3_Grapple_x2");
                        _wallFact = true;
                    }
                }
            }
        }

    }
}
