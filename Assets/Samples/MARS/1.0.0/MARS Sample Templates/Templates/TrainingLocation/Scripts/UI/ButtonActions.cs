using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.MARS.Templates.UI
{
    /// <summary>
    /// Helper class to manage various procedural effects and reactions of a button prefab
    /// </summary>
    internal class ButtonActions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The glow that shows up around the button and behind the text.")]
        Image m_Glow;

        [SerializeField]
        [Tooltip("The button background.")]
        RectTransform m_Background;

        [SerializeField]
        [Tooltip("The shdow image that is positioned behind the background.")]
        RectTransform m_BackgroundShadow;

        [SerializeField]
        [Tooltip("The icon on the button.")]
        RectTransform m_Icon;

        [SerializeField]
        [Tooltip("The shadow image placed behind the icon.")]
        RectTransform m_Shadow;

        [SerializeField]
        [Tooltip("A tooltip associated with the button")]
        RectTransform m_ButtonText;
#pragma warning restore 649

        [SerializeField]
        [Tooltip("How much to scale the button on hover.")]
        float m_ButtonScale = 1.25f;

        [SerializeField]
        [Tooltip("How much to translate the button imagery on the z on hover.")]
        float m_ZTranslation = -5f;

        [SerializeField]
        [Tooltip("How much to scale the button imagery on the y on hover.")]
        float m_YTranslation = 4f;

        [SerializeField]
        [Tooltip("How much to scale the background glow on hover.")]
        bool m_ScaleGlow = false;

        [SerializeField]
        [Tooltip("How much to scale the background image on hover.")]
        bool m_ScaleBackground = true;

        [SerializeField]
        [Tooltip("Whether or not to fade the icon shadow while hovering.")]
        bool m_FadeShadow = false;

        float m_InitialBackground;
        float m_InitialBackgroundShadow;
        float m_InitialYShadow;
        float m_InitialZIcon;
        AudioSource m_Sound;
        Image m_ShadowImage;

        void Awake()
        {
            m_Sound = gameObject.GetComponent<AudioSource>();
            if (m_ButtonText != null)
            {
                m_ButtonText.localScale = Vector3.zero;
            }

            if (m_Background != null)
            {
                m_InitialBackground = m_Background.localPosition.z;
                m_InitialBackgroundShadow = m_BackgroundShadow.localPosition.y;
                m_ShadowImage = m_BackgroundShadow.gameObject.GetComponent<Image>();
                if (m_FadeShadow)
                {
                    m_ShadowImage.CrossFadeAlpha(0f, 0.25f, true);
                }
            }

            if (m_Icon != null)
            {
                m_InitialYShadow = m_Shadow.localPosition.y;
                m_InitialZIcon = m_Icon.localPosition.z;
            }

            if (m_Glow != null)
            {
                m_Glow.color = new Color(1f, 1f, 1f, 0f);
                //m_Glow.CrossFadeAlpha(0f, 0.25f, true);
            }
        }

        void OnEnable()
        {
            PerformExitActions();
        }

        void OnDisable()
        {
            PerformExitActions();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PerformEntranceActions();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PerformExitActions();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PerformClickActions();
        }

        void PerformEntranceActions()
        {
            m_Sound.Play();

            if (m_Background != null)
            {
                if (m_ScaleBackground)
                {
                    m_BackgroundShadow.localScale = Vector3.one * m_ButtonScale;
                }

                var backgroundShadowLocalPosition = m_BackgroundShadow.localPosition;
                backgroundShadowLocalPosition.y = m_InitialBackgroundShadow - m_YTranslation;
                m_BackgroundShadow.localPosition = backgroundShadowLocalPosition;

                var backgroundLocalPosition = m_Background.localPosition;
                backgroundLocalPosition.z = m_InitialBackground + m_ZTranslation;
                m_Background.localPosition = backgroundLocalPosition;

                if (m_ScaleBackground)
                {
                    m_Background.localScale = Vector3.one * m_ButtonScale;
                }

                if (m_FadeShadow)
                {
                    m_ShadowImage.CrossFadeAlpha(1f, 0.25f, true);
                }
            }

            if (m_Icon != null)
            {
                var shadowLocalPosition = m_Shadow.localPosition;
                shadowLocalPosition.y = m_InitialYShadow - m_YTranslation;
                m_Shadow.localPosition = shadowLocalPosition;

                var iconLocalPosition = m_Icon.localPosition;
                iconLocalPosition.z = m_InitialZIcon + m_ZTranslation;
                m_Icon.localPosition = iconLocalPosition;
                m_Icon.localScale = Vector3.one * m_ButtonScale;
            }

            if (m_ButtonText != null)
            {
                m_ButtonText.localScale = Vector3.one;
            }

            if (m_Glow != null)
            {
                m_Glow.color = new Color(1f, 1f, 1f, 1f);
                //m_Glow.CrossFadeAlpha(.75f, 0.25f, true);

                if (m_ScaleGlow)
                {
                    m_Glow.gameObject.transform.localScale = Vector3.one * m_ButtonScale;
                }
            }
        }

        void PerformExitActions()
        {
            if (m_Background != null)
            {
                m_BackgroundShadow.localScale = Vector3.one;
                var backgroundShadowLocalPosition = m_BackgroundShadow.localPosition;
                backgroundShadowLocalPosition.y = m_InitialBackgroundShadow;
                m_BackgroundShadow.localPosition = backgroundShadowLocalPosition;

                var backgroundLocalPosition = m_Background.localPosition;
                backgroundLocalPosition.z = m_InitialBackground;
                m_Background.localPosition = backgroundLocalPosition;
                m_Background.localScale = Vector3.one;

                if (m_FadeShadow)
                {
                    m_ShadowImage.CrossFadeAlpha(0f, 0.25f, true);
                }
            }

            if (m_Icon != null)
            {
                m_Shadow.localScale = Vector3.one;
                var shadowLocalPosition = m_Shadow.localPosition;
                shadowLocalPosition.y = m_InitialYShadow;
                m_Shadow.localPosition = shadowLocalPosition;

                var iconLocalPosition = m_Icon.localPosition;
                iconLocalPosition.z = m_InitialZIcon;
                m_Icon.localPosition = iconLocalPosition;
                m_Icon.localScale = Vector3.one;
                iconLocalPosition.z = m_InitialZIcon;
            }

            if (m_ButtonText != null)
            {
                m_ButtonText.localScale = Vector3.zero;
            }

            if (m_Glow != null)
            {
                m_Glow.color = new Color(1f, 1f, 1f, 0f);
                //m_Glow.CrossFadeAlpha(0f, 0.25f, true);

                if (m_ScaleGlow)
                {
                    m_Glow.gameObject.transform.localScale = Vector3.one;
                }
            }
        }
        void PerformClickActions()
        {
            m_Sound.Play();

            if (m_Glow != null)
            {
                m_Glow.color = new Color(1f, 1f, 1f, 1f);
                //m_Glow.CrossFadeAlpha(0.75f, 0.25f, true);
            }
        }
    }
}
