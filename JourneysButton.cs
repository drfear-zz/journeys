using ColossalFramework;
using ColossalFramework.UI;
using System;
using UnityEngine;
using Journeys.RedirectionFramework;


namespace Journeys
{
    public class JourneysButton : UIPanel
    {
        private UIPanel _JPanel;
        public UIPanel _JButton;
        public UIButton _jbutton;
        public bool m_inJourneys;
        public static JourneysButton instance;


        public override void Awake()
        {
            Debug.Log("JV: started jButton awake");
            base.Awake();
            instance = this;
            SetupButton();
            m_inJourneys = false;
            m_moveInitialized = false;
            m_deltaPos = Vector3.zero;
            _JPanel = Singleton<JourneysPanel>.instance._JPanel;
            _JButton.Show();
            _JButton.BringToFront();
            Debug.Log("JV: done jButton awake");
        }



        public void SetupButton()
        {
            size = new Vector2(30, 30);
            UIPanel jButton = AddUIComponent<UIPanel>();
            jButton.canFocus = true;
            jButton.isInteractive = true;
            jButton.isVisible = true;
            jButton.width = 30f;
            jButton.height = 30f;
            jButton.anchor = UIAnchorStyle.All;
            jButton.pivot = UIPivotPoint.TopLeft;
            jButton.absolutePosition = new Vector3(180f, 18f);
            jButton.eventMouseMove += new MouseEventHandler(JButtonMouseMove);
            jButton.eventMouseUp += new MouseEventHandler(JButtonMouseUp);
            jButton.eventDoubleClick += new MouseEventHandler(JButtonDblClick);
            _JButton = jButton;
            UIButton jbutton = UIUtils.CreateButton((UIComponent)jButton);
            jbutton.textColor = Color.red;
            jbutton.size = new Vector2(30f, 30f);
            jbutton.relativePosition = Vector3.zero;
            jbutton.eventClick += new MouseEventHandler(JPanelToggle);
            jbutton.text = "J";
            _jbutton = jbutton;
        }

        private void JPanelToggle(UIComponent component, UIMouseEventParameter p)
        {
            if (_JPanel.isVisible)
                _JPanel.Hide();
            else
            {
                if (!Redirector<JourneyDetourer>.IsDeployed())
                {
                    if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes)
                    {
                        PathVisualizer thePV = Singleton<NetManager>.instance.PathVisualizer;
                        thePV.PathsVisible = false;     // mark paths not visible stops SimulationStep and RenderPaths from doing anything (Update() might still run but it does nothing on screen)
                        thePV.DestroyPaths();
                    }
                    Redirector<JourneyDetourer>.Deploy();
                    m_inJourneys = true;
                    InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.TrafficRoutes, InfoManager.SubInfoMode.Default);
                    UIView.library.Hide("TrafficRoutesInfoViewPanel");
                    _jbutton.textColor = Color.white;
                    _jbutton.Unfocus();
                }
                _JPanel.BringToFront();
                _JPanel.Show();
            }
        }

        public override void Update()
        {
            if (m_inJourneys && InfoManager.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes)
            {
                UIView.library.Hide("TrafficRoutesInfoViewPanel");  // this happens when eg user goes to public transport view then back to PV view
            }

        }

        private Vector3 m_deltaPos;
        private bool m_moveInitialized;

        private void JButtonMouseMove(UIComponent component, UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = m_OwnerView.fixedHeight - mousePos.y;
                if (!m_moveInitialized)
                {
                    m_deltaPos = _JButton.absolutePosition - mousePos;
                    m_moveInitialized = true;
                }
                else
                    _JButton.absolutePosition = mousePos + m_deltaPos;
            }
        }

        private void JButtonMouseUp(UIComponent component, UIMouseEventParameter p)
        {
            m_moveInitialized = false;
        }

        private void JButtonDblClick(UIComponent component, UIMouseEventParameter p)
        {
            _JPanel.absolutePosition = new Vector3(80f, 58f);
        }

        // MouseDown never gets called.  If it's on the panel, the button hides it, if it's on the button it's ignored

    }
}
