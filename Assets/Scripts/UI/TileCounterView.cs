using UnityEngine;
using UnityEngine.UI;

namespace IFC.Systems.UI
{
    public class TileCounterView : MonoBehaviour
    {
        [SerializeField] private Text label;

        public void Initialize(Text text)
        {
            label = text;
        }

        public void Refresh(CityViewModel viewModel)
        {
            if (label == null || !viewModel.IsValid)
            {
                return;
            }

            int used = viewModel.GetTileUsage();
            int cap = viewModel.GetTileCap();
            int th = viewModel.GetTownHallLevel();
            if (cap > 0)
            {
                label.text = $"Tiles: {used}/{cap} (TH{th})";
            }
            else
            {
                label.text = $"Tiles: {used} (TH{th})";
            }
        }
    }
}
