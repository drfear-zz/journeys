using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Journeys
{
    public class JourneysPanel : UIPanel
    {
        public static JourneysPanel instance;
        private UIView m_uiView;
        private Transform m_CameraTransform;
        private UIComponent m_FullscreenContainer;
        private UITextField m_name1;
        private UICheckBox m_perselection;
        private UICheckBox m_perlines;
        private UICheckBox m_pertype;


        public override void Awake()
        {
            Debug.Log("Called JP Awake");
            base.Awake();
            Debug.Log("Called base Awake");
            JourneysPanel instance = this;
            Debug.Log("Set instance OK");
            m_uiView = GetUIView();
            if ((UnityEngine.Object)Camera.main != (UnityEngine.Object)null)
                this.m_CameraTransform = Camera.main.transform;
            this.m_FullscreenContainer = UIView.Find("FullScreenContainer");
            this.SetupPanel();
        }

        public void SetupPanel()
        {
            name = "JourneysPanel";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            anchor = UIAnchorStyle.None;
            pivot = UIPivotPoint.BottomLeft;
            width = 400f;
            height = 400f;
            backgroundSprite = "GenericPanelGrey";
            UIPanel uiPanel1 = AddUIComponent<UIPanel>();
            uiPanel1.name = "JourneysPanelTitle";
            uiPanel1.height = 40f;
            Vector3 vector3_1 = new Vector3(0.0f, 0.0f);
            uiPanel1.relativePosition = vector3_1;
            UITextField uiTextField = uiPanel1.AddUIComponent<UITextField>();
            uiTextField.name = "TitleText";
            //uiTextField.font = UIUtils.Font;
            uiTextField.height = 25f;
            uiTextField.width = 200f;
            uiTextField.maxLength = 32;
            uiTextField.padding = new RectOffset(0, 0, 4, 0);
            uiTextField.verticalAlignment = UIVerticalAlignment.Middle;
            uiTextField.position = new Vector3((float)((double)this.width / 2.0 - (double)uiTextField.width / 2.0),
                (float)((double)uiTextField.height / 2.0 - 20.0));
            m_name1 = uiTextField;
            m_name1.text = "Journey Viewer";

            // checkboxes to control the coloring of journeys
            UIPanel colouring = AddUIComponent<UIPanel>();
            colouring.name = "ColouringRadioPanel";
            colouring.height = 60f;
            colouring.width = 200f;
            colouring.relativePosition = new Vector3(0f, 40f);

            UICheckBox tmp = colouring.AddUIComponent<UICheckBox>();
            m_perselection = tmp;
            m_perselection.relativePosition = new Vector3(5f, 10f);
            m_perselection.canFocus = true;
            m_perlines = colouring.AddUIComponent<UICheckBox>();
            m_perlines.relativePosition = new Vector3(5f, 30f);
            m_perlines.canFocus = true;
            m_pertype = colouring.AddUIComponent<UICheckBox>();
            m_pertype.relativePosition = new Vector3(5f, 50f);
            m_pertype.canFocus = true;

            this.m_perselection.eventCheckChanged += (PropertyChangedEventHandler<bool>)((c, r) =>
            {
                Singleton<SimulationManager>.instance.AddAction((System.Action)(() =>
                {
                    if (r)
                    {
                        m_perselection.isChecked = true;
                        m_perlines.isChecked = false;
                        m_pertype.isChecked = false;
                    }
                    else
                    {
                        m_perselection.isChecked = true;    // you cannot uncheck directly, this is a radio group
                    }
                }));
            });
            this.m_perlines.eventCheckChanged += (PropertyChangedEventHandler<bool>)((c, r) =>
            {
                Singleton<SimulationManager>.instance.AddAction((System.Action)(() =>
                {
                    if (r)
                    {
                        m_perselection.isChecked = false;
                        m_perlines.isChecked = true;
                        m_pertype.isChecked = false;
                    }
                    else
                    {
                        m_perlines.isChecked = true;    // you cannot uncheck directly, this is a radio group
                    }
                }));
            });
            this.m_pertype.eventCheckChanged += (PropertyChangedEventHandler<bool>)((c, r) =>
            {
                Singleton<SimulationManager>.instance.AddAction((System.Action)(() =>
                {
                    if (r)
                    {
                        m_perselection.isChecked = false;
                        m_perlines.isChecked = false;
                        m_pertype.isChecked = true;
                    }
                    else
                    {
                        m_pertype.isChecked = true;    // you cannot uncheck directly, this is a radio group
                    }
                }));
            });

            //UIScrollablePanel scrpanel = AddUIComponent<UIScrollablePanel>();
            //scrpanel.name = "test";
            //scrpanel.height = 200f;
            //scrpanel.width = 200f;
            //scrpanel.position = new Vector3(0f, 140f);
            //for (int i = 1; i < 21; i++)
            //{
            //    UITextField tbox = scrpanel.AddUIComponent<UITextField>();
            //    tbox.height = 25f;
            //    tbox.width = 200f;
            //    tbox.text = "Citizen " + i;
            //}
            //UITabstrip ts = Add
        }


        //public void Init()
        //{
        //    Debug.Log("Called JP Init");
        //    JourneysPanel instance = this;
        //    m_label = Find<UILabel>("");
        //    m_Strip = Find<UITabstrip>("Tabstrip");
        //    m_CitizensContainer = Find("Citizens").Find("Container");
        //    m_LinesContainer = Find("JourneyLines").Find("Container");
        //}

        //public void Show()
        //{
        //    this.Show();
        //}

    }
}
