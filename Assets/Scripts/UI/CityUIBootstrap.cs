using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IFC.Systems.UI
{
    [DisallowMultipleComponent]
    public class CityUIBootstrap : MonoBehaviour
    {
        [SerializeField] private GameLoop gameLoop;
        [SerializeField] private string cityId = "Capital";

        private void Awake()
        {
            if (gameLoop == null)
            {
                gameLoop = FindObjectOfType<GameLoop>();
            }

            BuildUI();
        }

        private void BuildUI()
        {
            if (FindObjectOfType<CityUIController>() != null)
            {
                return;
            }

            var canvasGO = new GameObject("CityUICanvas");
            canvasGO.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasGO.AddComponent<GraphicRaycaster>();

            var controller = canvasGO.AddComponent<CityUIController>();

            // Top Bar
            var topBar = CreatePanel(canvasGO.transform, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 80f), new Vector2(0.5f, 1f));
            var topBarLayout = topBar.AddComponent<HorizontalLayoutGroup>();
            topBarLayout.childForceExpandHeight = false;
            topBarLayout.childForceExpandWidth = false;
            topBarLayout.spacing = 8f;
            var resourcesText = CreateText(topBar.transform, "ResourcesText", "Resources");
            var resourceView = topBar.AddComponent<ResourceBarView>();
            resourceView.Initialize(resourcesText);

            // City Panel
            var cityPanel = CreatePanel(canvasGO.transform, "CityPanel", new Vector2(0f, 0f), new Vector2(0.35f, 0.8f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            var vertical = cityPanel.AddComponent<VerticalLayoutGroup>();
            vertical.childForceExpandHeight = false;
            vertical.childForceExpandWidth = true;
            vertical.spacing = 6f;

            string[] buildings = { "TownHall", "GeneralHQ", "MilitaryInstitute", "Wall" };
            var cardViews = new System.Collections.Generic.List<BuildingCardView>();
            for (int i = 0; i < buildings.Length; i++)
            {
                var card = CreatePanel(cityPanel.transform, $"Card_{buildings[i]}", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 140f), new Vector2(0f, 0f));
                var image = card.AddComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0.3f);
                var layout = card.AddComponent<VerticalLayoutGroup>();
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
                layout.spacing = 2f;

                var nameText = CreateText(card.transform, "Name", buildings[i]);
                var levelText = CreateText(card.transform, "Level", "Level 0");
                var costText = CreateText(card.transform, "Cost", "Cost");
                var etaText = CreateText(card.transform, "ETA", "00:00");
                var lockText = CreateText(card.transform, "Lock", "");
                lockText.color = Color.yellow;
                var buttonGO = new GameObject("UpgradeButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonGO.transform.SetParent(card.transform, false);
                var buttonImage = buttonGO.GetComponent<Image>();
                buttonImage.color = new Color(0.2f, 0.5f, 0.2f, 0.8f);
                var buttonRect = buttonGO.GetComponent<RectTransform>();
                buttonRect.sizeDelta = new Vector2(0, 36f);
                var button = buttonGO.GetComponent<Button>();
                var buttonLabel = CreateText(buttonGO.transform, "Label", "Upgrade");
                buttonLabel.alignment = TextAnchor.MiddleCenter;
                var cardView = card.AddComponent<BuildingCardView>();
                cardView.Initialize(nameText, levelText, costText, etaText, lockText, button);
                cardViews.Add(cardView);
            }

            // Footer
            var footer = CreatePanel(canvasGO.transform, "Footer", new Vector2(0.3f, 0f), new Vector2(0.7f, 0f), new Vector2(0f, 60f), new Vector2(0.5f, 0f));
            var footerText = CreateText(footer.transform, "TileCounter", "Tiles");
            footerText.alignment = TextAnchor.MiddleCenter;
            var tileCounter = footer.AddComponent<TileCounterView>();
            tileCounter.Initialize(footerText);

            // Toast
            var toastGO = CreatePanel(canvasGO.transform, "Toast", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f));
            var toastCanvas = toastGO.AddComponent<CanvasGroup>();
            var toastText = CreateText(toastGO.transform, "ToastLabel", "");
            toastText.alignment = TextAnchor.MiddleCenter;
            var toast = toastGO.AddComponent<ToastView>();
            toast.Initialize(toastText);
            toastCanvas.alpha = 0f;

            controller.Initialize(gameLoop, cityId, resourceView, tileCounter, toast, cardViews);
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            return go;
        }

        private static Text CreateText(Transform parent, string name, string value)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 24;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 24f);
            return text;
        }
    }
}
