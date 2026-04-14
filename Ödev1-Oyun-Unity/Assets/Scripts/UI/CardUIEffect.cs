using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace EGKart.UI
{
    /// <summary>
    /// Kartların hover (üzerine gelme) efektlerini yöneten yardımcı sınıf.
    /// </summary>
    public class CardUIEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Vector3 _originalScale;
        private Vector3 _originalPos;
        private RectTransform _rectTransform;
        
        private float _hoverScale = 1.2f;
        private float _hoverYOffset = 50f;
        private float _animationSpeed = 10f;

        private bool _isHovered = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
            _originalPos = _rectTransform.localPosition;
        }

        public void SetOriginalState(Vector3 pos, Vector3 scale)
        {
            _originalPos = pos;
            _originalScale = scale;
            if (!_isHovered)
            {
                _rectTransform.localPosition = pos;
                _rectTransform.localScale = scale;
            }
        }

        private void Update()
        {
            Vector3 targetScale = _isHovered ? _originalScale * _hoverScale : _originalScale;
            Vector3 targetPos = _isHovered ? _originalPos + new Vector3(0, _hoverYOffset, 0) : _originalPos;

            _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, targetScale, Time.deltaTime * _animationSpeed);
            _rectTransform.localPosition = Vector3.Lerp(_rectTransform.localPosition, targetPos, Time.deltaTime * _animationSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            transform.SetAsLastSibling(); // En öne getir
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
        }
    }
}
