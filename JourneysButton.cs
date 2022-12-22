using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Journeys.RedirectionFramework;


namespace Journeys
{

    public class JourneysPanel : UIPanel
    {
        //private UIView m_uiView;
        //private Transform m_CameraTransform;
        //private UIComponent m_FullscreenContainer;
        public static JourneysPanel instance;
        private UILabel m_name1;
        private UILabel _numSelectedLabel;
        private UILabel _numSubselectedLabel;
        private UICheckBox _fromChk;
        private UICheckBox _toChk;
        private UICheckBox _ptChk;
        private UICheckBox _resiChk;
        private UICheckBox _tourChk;
        private UICheckBox _ptLaneChk;
        private UITextField[] _cutBoxes;
        private UICheckBox[] _minChks;
        private UIButton _colouringBtn;
        private UICheckBox _blendChk;
        private UICheckBox _stopsChk;
        private UICheckBox _carsChk;
        private UITextField _maxtext;
        private UIButton _secondABBtn;
        private UICheckBox _modeABChk;
        private UIButton _plusBtn;
        private UICheckBox _plusChk;
        public UIPanel _JPanel;
        //public UIPanel _JButton;

        //public Dictionary<int, Color> m_travelModeColors;

        //public int NumSelected {get; set; }
        //public int NumSubselected { get; set; }


        public override void Awake()
        {

            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            base.Awake();
            //m_uiView = GetUIView();
            //if ((UnityEngine.Object)Camera.main != (UnityEngine.Object)null)
            //    this.m_CameraTransform = Camera.main.transform;
            //m_FullscreenContainer = UIView.Find("FullScreenContainer");
            //SetupColors();
            //NumSelected = 0;
            //NumSubselected = 0;
            instance = this;
            _minChks = new UICheckBox[9];
            _cutBoxes = new UITextField[9];
            SetupPanel();
            //SetupButton();
            UpdateBothSelected(0, 0);
            _toChk.isChecked = false;
            _fromChk.isChecked = false;
            _ptChk.isChecked = false;
            _ptLaneChk.isChecked = true;
            _minChks[1].isChecked = true;
            _blendChk.isChecked = false;
            _stopsChk.isChecked = false;
            _carsChk.isChecked = false;
            _resiChk.isChecked = false;
            _tourChk.isChecked = false;

            for (int idx = 1; idx < 9; idx++)
            {
                _cutBoxes[idx].text = "" + JVutils.cutoffs[idx];
            }

            //Hide();
            _JPanel.Hide();
            Debug.Log("done panel awake");
            //_JButton.Show();
        }


        //public void SetupColors()
        //{
        //    m_travelModeColors = new Dictionary<int, Color>();
        //    m_travelModeColors.Add(0, Color.white);  // representing serious error condition (segment info null)
        //    m_travelModeColors.Add(1, ColorUtils.Desaturate(Color.magenta, 0.3f));  // cars
        //    m_travelModeColors.Add(2, ColorUtils.Desaturate(Color.green, 0.3f));  // pedestrians
        //    m_travelModeColors.Add(3, Color.grey);  // bicycles
        //    m_travelModeColors.Add(4, Color.black);  // all other non-public-transport (mostly never selectable)
        //    m_travelModeColors.Add(5, Color.blue);  // generic for bus
        //    m_travelModeColors.Add(6, Color.green);  // metro
        //    m_travelModeColors.Add(7, Color.yellow);  // train
        //    m_travelModeColors.Add(8, Color.yellow);  // ship
        //    m_travelModeColors.Add(9, Color.yellow);  // airplane
        //    m_travelModeColors.Add(10, Color.yellow);  // taxi
        //    m_travelModeColors.Add(11, Color.magenta);  // tram
        //    m_travelModeColors.Add(12, Color.yellow);  // evac bus
        //    m_travelModeColors.Add(13, Color.yellow);  // monorail
        //    m_travelModeColors.Add(14, Color.yellow);  // cablecar
        //    m_travelModeColors.Add(15, Color.yellow);  // touristbus
        //    m_travelModeColors.Add(16, Color.yellow);  // hotairballon
        //    m_travelModeColors.Add(17, Color.yellow);  // post
        //    m_travelModeColors.Add(18, Color.yellow);  // trolleybus
        //    m_travelModeColors.Add(19, Color.yellow);  // fishing
        //    m_travelModeColors.Add(20, Color.yellow);  // helicopter


        //}

        public void UpdateBothSelected(int numselected, int numsubselected, string jqualifier = " journeys selected")
        {
            _numSelectedLabel.text = numselected + jqualifier;
            _numSubselectedLabel.text = numsubselected + " journeys in subselection";
        }
        //public void UpdateSelected(int num)
        //{
        //    NumSelected = num;
        //    _numSelectedLabel.text = NumSelected + " journeys selected";
        //}

        //public void UpdateSubselected(int num)
        //{
        //    NumSubselected = num;
        //    _numSubselectedLabel.text = NumSubselected + " journeys in subselection";
        //}

        public void SetupPanel()
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            Color32 bblue = new Color32(185, 221, 254, 255);

            size = Vector2.zero;
            absolutePosition = new Vector3(-1000, -1000);
            UIPanel uiPanel0 = AddUIComponent<UIPanel>();
            uiPanel0.name = "JourneysPanel";
            uiPanel0.isVisible = true;
            uiPanel0.canFocus = true;
            uiPanel0.isInteractive = true;
            uiPanel0.anchor = UIAnchorStyle.None;
            uiPanel0.pivot = UIPivotPoint.TopLeft;
            uiPanel0.width = 360f;
            uiPanel0.height = 800f;
            //uiPanel0.relativePosition = new Vector3(80f, 58f);
            uiPanel0.absolutePosition = new Vector3(80f, 58f);
            uiPanel0.eventMouseDown += new MouseEventHandler(JPanelMouseDown);
            uiPanel0.eventMouseMove += new MouseEventHandler(JPanelMouseMove);
            uiPanel0.eventDoubleClick += new MouseEventHandler(JPanelDblClick);
            _JPanel = uiPanel0;
            //backgroundSprite = null;
            //backgroundSprite = "BuffPanel";
            //backgroundSprite = "GenericPanel";
            //backgroundSprite = "InfoBubbleVehicle";
            //backgroundSprite = "GenericPanelLight";
            //backgroundSprite = "GenericPanel";
            //backgroundSprite = "MenuPanel2";
            //backgroundSprite = "UnlockingPanel2";
            uiPanel0.backgroundSprite = "SubcategoriesPanel";
            UIPanel uiPanel1 = uiPanel0.AddUIComponent<UIPanel>();
            uiPanel1.name = "JourneysPanelTitle";
            uiPanel1.height = 40f;
            uiPanel1.relativePosition = Vector3.zero;
            UILabel uiTitle = uiPanel1.AddUIComponent<UILabel>();
            uiTitle.name = "TitleText";
            //uiTextField.font = UIUtils.Font;
            uiTitle.height = 25f;
            uiTitle.width = 200f;
            //uiTitle.maxLength = 32;
            uiTitle.padding = new RectOffset(0, 0, 4, 0);
            uiTitle.textAlignment = UIHorizontalAlignment.Center;
            //uiTitle.verticalAlignment = UIVerticalAlignment.Middle;
            uiTitle.relativePosition = new Vector3(110, 7.5f);
            m_name1 = uiTitle;
            m_name1.text = "Journey Viewer";

            UIPanel uiPanel2 = uiPanel0.AddUIComponent<UIPanel>();
            uiPanel2.name = "Container";
            uiPanel2.width = 328f;
            uiPanel2.height = 341f;
            uiPanel2.autoLayout = true;
            uiPanel2.autoLayoutDirection = (LayoutDirection)1;
            uiPanel2.autoLayoutPadding = new RectOffset(10, 10, 5, 0);
            uiPanel2.autoLayoutStart = (LayoutStart)0;
            uiPanel2.relativePosition = new Vector3(6f, 46f);

            UIPanel uiPanel3 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel3.name = "Selected";
            uiPanel3.anchor = (UIAnchorStyle)13;
            uiPanel3.autoLayout = true;
            uiPanel3.autoLayoutDirection = (LayoutDirection)0;
            uiPanel3.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel3.autoLayoutStart = (LayoutStart)0;
            uiPanel3.size = new Vector2(328f, 18f);
            UILabel numselected = uiPanel3.AddUIComponent<UILabel>();
            numselected.name = "SelectedText";
            //uiTextField.font = UIUtils.Font;
            numselected.autoSize = true;
            numselected.height = 18f;
            //numselected.textScale = 14f / 16f;
            numselected.textColor = bblue;
            numselected.font = UIUtils.Font;
            _numSelectedLabel = numselected;
            //_numSelectedLabel.text = NumSelected + " journeys selected";

            UIPanel uiPanel3a = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel3a.size = new Vector2(328, 9);

            UIPanel uiPanel4 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel4.name = "Subselected";
            uiPanel4.anchor = (UIAnchorStyle)13;
            uiPanel4.autoLayout = true;
            uiPanel4.autoLayoutDirection = (LayoutDirection)0;
            uiPanel4.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel4.autoLayoutStart = (LayoutStart)0;
            uiPanel4.size = new Vector2(328f, 18f);
            UILabel numsubselected = uiPanel4.AddUIComponent<UILabel>();
            numselected.name = "SubselectedText";
            //uiTextField.font = UIUtils.Font;
            numsubselected.autoSize = true;
            numsubselected.height = 18f;
            //numsubselected.textScale = 14f / 16f;
            numsubselected.textColor = bblue;
            numsubselected.font = UIUtils.Font;
            _numSubselectedLabel = numsubselected;
            //_numSubselectedLabel.text = NumSubselected + " journeys selected";

            UIPanel uiPanel4a = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel4a.size = new Vector2(328, 12);

            UIPanel uiPanel5 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel5.name = "FromToHere";
            uiPanel5.anchor = (UIAnchorStyle)13;
            uiPanel5.autoLayout = true;
            uiPanel5.autoLayoutDirection = (LayoutDirection)0;
            uiPanel5.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel5.autoLayoutStart = (LayoutStart)0;
            uiPanel5.size = new Vector2(328f, 20f);
            //uiPanel5.useCenter = true;
            UICheckBox fromChk = uiPanel5.AddUIComponent<UICheckBox>();
            fromChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            fromChk.clipChildren = true;
            fromChk.eventClicked += new MouseEventHandler(FromChkClick);
            UISprite uiSprite2 = fromChk.AddUIComponent<UISprite>();
            uiSprite2.spriteName = "check-unchecked";
            uiSprite2.size = new Vector2(16f, 16f);
            uiSprite2.relativePosition = Vector3.zero;
            fromChk.checkedBoxObject = (UIComponent)uiSprite2.AddUIComponent<UISprite>();
            ((UISprite)fromChk.checkedBoxObject).spriteName = "check-checked";
            fromChk.checkedBoxObject.size = new Vector2(16f, 16f);
            fromChk.checkedBoxObject.relativePosition = Vector3.zero;
            fromChk.label = fromChk.AddUIComponent<UILabel>();
            //fromChk.label.font = UIUtils.Font;
            fromChk.label.textColor = bblue;
            fromChk.label.disabledTextColor = (Color32)Color.black;
            fromChk.label.textScale = 13f / 16f;
            fromChk.label.text = "show only From here";
            fromChk.label.relativePosition = new Vector3(22f, 2f);
            fromChk.size = new Vector2(fromChk.label.width + 22f, 16f);
            _fromChk = fromChk;
            UICheckBox toChk = uiPanel5.AddUIComponent<UICheckBox>();
            // toChk.relativePosition = new Vector2(190f, 0f);
            toChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            toChk.clipChildren = true;
            toChk.eventClicked += new MouseEventHandler(ToChkClick);
            UISprite uiSprite3 = toChk.AddUIComponent<UISprite>();
            uiSprite3.spriteName = "check-unchecked";
            uiSprite3.size = new Vector2(16f, 16f);
            uiSprite3.relativePosition = Vector3.zero;
            toChk.checkedBoxObject = (UIComponent)uiSprite3.AddUIComponent<UISprite>();
            ((UISprite)toChk.checkedBoxObject).spriteName = "check-checked";
            toChk.checkedBoxObject.size = new Vector2(16f, 16f);
            toChk.checkedBoxObject.relativePosition = Vector3.zero;
            toChk.label = toChk.AddUIComponent<UILabel>();
            //fromChk.label.font = UIUtils.Font;
            toChk.label.textColor = bblue;
            toChk.label.disabledTextColor = (Color32)Color.black;
            toChk.label.textScale = 13f / 16f;
            toChk.label.relativePosition = new Vector3(22f, 2f);
            toChk.label.text = "show only To here";
            toChk.size = new Vector2(toChk.label.width + 22f, 16f);
            _toChk = toChk;

            UIPanel uiPanel6 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel6.name = "PTstretches";
            uiPanel6.anchor = (UIAnchorStyle)13;
            uiPanel6.autoLayout = true;
            uiPanel6.autoLayoutDirection = (LayoutDirection)0;
            uiPanel6.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel6.autoLayoutStart = (LayoutStart)0;
            uiPanel6.size = new Vector2(328f, 22f);
            uiPanel6.useCenter = true;
            UICheckBox ptChk = uiPanel6.AddUIComponent<UICheckBox>();
            ptChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            ptChk.clipChildren = true;
            ptChk.eventClicked += new MouseEventHandler(PTstretchesToggled);
            UISprite uiSprite4 = ptChk.AddUIComponent<UISprite>();
            uiSprite4.spriteName = "check-unchecked";
            uiSprite4.size = new Vector2(16f, 16f);
            uiSprite4.relativePosition = Vector3.zero;
            ptChk.checkedBoxObject = (UIComponent)uiSprite4.AddUIComponent<UISprite>();
            ((UISprite)ptChk.checkedBoxObject).spriteName = "check-checked";
            ptChk.checkedBoxObject.size = new Vector2(16f, 16f);
            ptChk.checkedBoxObject.relativePosition = Vector3.zero;
            ptChk.label = ptChk.AddUIComponent<UILabel>();
            //fromChk.label.font = UIUtils.Font;
            ptChk.label.textColor = bblue;
            ptChk.label.disabledTextColor = (Color32)Color.black;
            ptChk.label.textScale = 13f / 16f;
            ptChk.label.text = "show only PT stretches and transfers";
            ptChk.label.relativePosition = new Vector3(22f, 2f);
            ptChk.size = new Vector2(ptChk.label.width + 22f, 16f);
            _ptChk = ptChk;

            UIPanel tourism = uiPanel2.AddUIComponent<UIPanel>();
            tourism.name = "Tourism";
            tourism.anchor = (UIAnchorStyle)13;
            tourism.autoLayout = true;
            tourism.autoLayoutDirection = (LayoutDirection)0;
            tourism.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            tourism.autoLayoutStart = (LayoutStart)0;
            tourism.size = new Vector2(328f, 20f);
            UICheckBox resiChk = tourism.AddUIComponent<UICheckBox>();
            resiChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            resiChk.clipChildren = true;
            resiChk.eventClicked += new MouseEventHandler(ResiChkClick);
            UISprite resiSprite = resiChk.AddUIComponent<UISprite>();
            resiSprite.spriteName = "check-unchecked";
            resiSprite.size = new Vector2(16f, 16f);
            resiSprite.relativePosition = Vector3.zero;
            resiChk.checkedBoxObject = (UIComponent)resiSprite.AddUIComponent<UISprite>();
            ((UISprite)resiChk.checkedBoxObject).spriteName = "check-checked";
            resiChk.checkedBoxObject.size = new Vector2(16f, 16f);
            resiChk.checkedBoxObject.relativePosition = Vector3.zero;
            resiChk.label = resiChk.AddUIComponent<UILabel>();
            //resiChk.label.font = UIUtils.Font;
            resiChk.label.textColor = bblue;
            resiChk.label.disabledTextColor = (Color32)Color.black;
            resiChk.label.textScale = 13f / 16f;
            resiChk.label.text = "show only residents";
            resiChk.label.relativePosition = new Vector3(22f, 2f);
            resiChk.size = new Vector2(resiChk.label.width + 22f, 16f);
            _resiChk = resiChk;
            UICheckBox tourChk = tourism.AddUIComponent<UICheckBox>();
            // tourChk.relativePosition = new Vector2(190f, 0f);
            tourChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            tourChk.clipChildren = true;
            tourChk.eventClicked += new MouseEventHandler(TourChkClick);
            UISprite tourSprite = tourChk.AddUIComponent<UISprite>();
            tourSprite.spriteName = "check-unchecked";
            tourSprite.size = new Vector2(16f, 16f);
            tourSprite.relativePosition = Vector3.zero;
            tourChk.checkedBoxObject = (UIComponent)tourSprite.AddUIComponent<UISprite>();
            ((UISprite)tourChk.checkedBoxObject).spriteName = "check-checked";
            tourChk.checkedBoxObject.size = new Vector2(16f, 16f);
            tourChk.checkedBoxObject.relativePosition = Vector3.zero;
            tourChk.label = tourChk.AddUIComponent<UILabel>();
            //resiChk.label.font = UIUtils.Font;
            tourChk.label.textColor = bblue;
            tourChk.label.disabledTextColor = (Color32)Color.black;
            tourChk.label.textScale = 13f / 16f;
            tourChk.label.relativePosition = new Vector3(22f, 2f);
            tourChk.label.text = "show only tourists";
            tourChk.size = new Vector2(tourChk.label.width + 22f, 16f);
            _tourChk = tourChk;



            UIPanel blank1 = uiPanel2.AddUIComponent<UIPanel>();
            blank1.size = new Vector2(328, 8);

            UIPanel uiPanel7 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel7.name = "Cycles";
            uiPanel7.anchor = (UIAnchorStyle)13;
            uiPanel7.autoLayout = true;
            uiPanel7.autoLayoutDirection = (LayoutDirection)0;
            uiPanel7.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel7.autoLayoutStart = (LayoutStart)0;
            uiPanel7.size = new Vector2(328f, 18f);
            uiPanel7.useCenter = true;
            UIButton uiButton1 = UIUtils.CreateButton((UIComponent)uiPanel7);
            uiButton1.name = "SelectLaneLine";
            uiButton1.textColor = bblue;
            uiButton1.textScale = 11f / 16f;
            uiButton1.font = ptChk.label.font;
            uiButton1.size = new Vector2(180f, 18f);
            uiButton1.relativePosition = Vector3.zero;
            uiButton1.eventClick += new MouseEventHandler(LaneCycleBtn);
            uiButton1.text = "Cycle through lane/line";
            UIButton uiButton2 = UIUtils.CreateButton((UIComponent)uiPanel7);
            uiButton2.font = ptChk.label.font;
            uiButton2.name = "ReverseLaneLine";
            uiButton2.textColor = bblue;
            uiButton2.textScale = 11f / 16f;
            uiButton2.size = new Vector2(140f, 18f);
            uiButton2.relativePosition = new Vector2(260f, 0f);
            uiButton2.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            //uiButton2.relativePosition = new Vector2(240f, 0f);
            uiButton2.eventClick += new MouseEventHandler(LaneCycleReverseBtn);
            uiButton2.text = "Reverse cycle";

            UIPanel uiPanel8 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel8.name = "PTstretches";
            uiPanel8.anchor = (UIAnchorStyle)13;
            uiPanel8.autoLayout = true;
            uiPanel8.autoLayoutDirection = (LayoutDirection)0;
            uiPanel8.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel8.autoLayoutStart = (LayoutStart)0;
            uiPanel8.size = new Vector2(328f, 16f);
            //uiPanel8.relativePosition = new Vector2(20f, 0f);
            UILabel blank2 = uiPanel8.AddUIComponent<UILabel>();
            blank2.size = new Vector2(32, 16);
            UICheckBox ptLaneChk = uiPanel8.AddUIComponent<UICheckBox>();
            ptLaneChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            ptLaneChk.clipChildren = true;
            ptLaneChk.eventClicked += new MouseEventHandler(PTLaneChkClick);
            UISprite uiSprite5 = ptLaneChk.AddUIComponent<UISprite>();
            uiSprite5.spriteName = "check-unchecked";
            uiSprite5.size = new Vector2(16f, 16f);
            uiSprite5.relativePosition = Vector3.zero;
            ptLaneChk.checkedBoxObject = (UIComponent)uiSprite5.AddUIComponent<UISprite>();
            ((UISprite)ptLaneChk.checkedBoxObject).spriteName = "check-checked";
            ptLaneChk.checkedBoxObject.size = new Vector2(16f, 16f);
            ptLaneChk.checkedBoxObject.relativePosition = Vector3.zero;
            ptLaneChk.label = ptLaneChk.AddUIComponent<UILabel>();
            //ptLaneChk.label.font = UIUtils.Font;
            ptLaneChk.label.textColor = bblue;
            ptLaneChk.label.disabledTextColor = (Color32)Color.black;
            ptLaneChk.label.textScale = 0.75f;
            ptLaneChk.label.text = "cycle only through PT lanes/lines";
            ptLaneChk.label.relativePosition = new Vector3(22f, 2f);
            ptLaneChk.size = new Vector2(ptLaneChk.label.width + 22f, 16f);
            _ptLaneChk = ptLaneChk;


            UIPanel blankAB = uiPanel2.AddUIComponent<UIPanel>();
            blankAB.size = new Vector2(328, 8);

            UIButton secondABBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            secondABBtn.textColor = bblue;
            secondABBtn.textScale = 0.75f;
            secondABBtn.size = new Vector2(328f, 18f);
            secondABBtn.eventClick += new MouseEventHandler(SecondABBtn);
            //secondABBtn.color = Color.red;
            secondABBtn.text = "make a secondary subselection";
            _secondABBtn = secondABBtn;
            UIPanel ABmodePanel = uiPanel2.AddUIComponent<UIPanel>();
            ABmodePanel.anchor = (UIAnchorStyle)13;
            ABmodePanel.autoLayout = true;
            ABmodePanel.autoLayoutDirection = (LayoutDirection)0;
            ABmodePanel.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            ABmodePanel.autoLayoutStart = (LayoutStart)0;
            ABmodePanel.size = new Vector2(328f, 16f);
            UILabel blankAB2 = ABmodePanel.AddUIComponent<UILabel>();
            blankAB2.size = new Vector2(32, 16);
            UICheckBox modeABChk = ABmodePanel.AddUIComponent<UICheckBox>();
            modeABChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            modeABChk.clipChildren = true;
            modeABChk.eventClicked += new MouseEventHandler(ModeABChkClick);
            UISprite uiSpriteAB = modeABChk.AddUIComponent<UISprite>();
            uiSpriteAB.spriteName = "check-unchecked";
            uiSpriteAB.size = new Vector2(16f, 16f);
            uiSpriteAB.relativePosition = Vector3.zero;
            modeABChk.checkedBoxObject = (UIComponent)uiSpriteAB.AddUIComponent<UISprite>();
            ((UISprite)modeABChk.checkedBoxObject).spriteName = "check-checked";
            modeABChk.checkedBoxObject.size = new Vector2(16f, 16f);
            modeABChk.checkedBoxObject.relativePosition = Vector3.zero;
            modeABChk.label = modeABChk.AddUIComponent<UILabel>();
            modeABChk.label.textColor = bblue;
            modeABChk.label.disabledTextColor = (Color32)Color.black;
            modeABChk.label.textScale = 0.75f;
            modeABChk.label.text = "remain in secondary selection mode";
            modeABChk.label.relativePosition = new Vector3(22f, 2f);
            modeABChk.size = new Vector2(modeABChk.label.width + 22f, 16f);
            modeABChk.isChecked = false;
            modeABChk.Disable();
            _modeABChk = modeABChk;


            UIButton plusBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            plusBtn.textColor = bblue;
            plusBtn.textScale = 0.75f;
            plusBtn.size = new Vector2(328f, 18f);
            plusBtn.eventClick += new MouseEventHandler(PlusBtn);
            //plusBtn.color = Color.red;
            plusBtn.text = "extend the selection";
            _plusBtn = plusBtn;
            UIPanel plusmodePanel = uiPanel2.AddUIComponent<UIPanel>();
            plusmodePanel.anchor = (UIAnchorStyle)13;
            plusmodePanel.autoLayout = true;
            plusmodePanel.autoLayoutDirection = (LayoutDirection)0;
            plusmodePanel.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            plusmodePanel.autoLayoutStart = (LayoutStart)0;
            plusmodePanel.size = new Vector2(328f, 16f);
            UILabel blankplus = plusmodePanel.AddUIComponent<UILabel>();
            blankplus.size = new Vector2(32, 16);
            UICheckBox plusChk = plusmodePanel.AddUIComponent<UICheckBox>();
            plusChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            plusChk.clipChildren = true;
            plusChk.eventClicked += new MouseEventHandler(PlusChkClick);
            UISprite plusSprite = plusChk.AddUIComponent<UISprite>();
            plusSprite.spriteName = "check-unchecked";
            plusSprite.size = new Vector2(16f, 16f);
            plusSprite.relativePosition = Vector3.zero;
            plusChk.checkedBoxObject = (UIComponent)plusSprite.AddUIComponent<UISprite>();
            ((UISprite)plusChk.checkedBoxObject).spriteName = "check-checked";
            plusChk.checkedBoxObject.size = new Vector2(16f, 16f);
            plusChk.checkedBoxObject.relativePosition = Vector3.zero;
            plusChk.label = plusChk.AddUIComponent<UILabel>();
            plusChk.label.textColor = bblue;
            plusChk.label.disabledTextColor = (Color32)Color.black;
            plusChk.label.textScale = 0.75f;
            plusChk.label.text = "remain in extend-selection mode";
            plusChk.label.relativePosition = new Vector3(22f, 2f);
            plusChk.size = new Vector2(plusChk.label.width + 22f, 16f);
            plusChk.isChecked = false;
            plusChk.Disable();
            _plusChk = plusChk;


            UIPanel blank3 = uiPanel2.AddUIComponent<UIPanel>();
            blank3.size = new Vector2(328, 16);

            UIPanel heatmap = uiPanel2.AddUIComponent<UIPanel>();
            heatmap.name = "HeatMapInfo";
            heatmap.autoLayout = true;
            heatmap.autoLayoutDirection = LayoutDirection.Vertical;
            heatmap.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            heatmap.autoLayoutStart = LayoutStart.TopLeft;
            heatmap.size = new Vector2(328f, 188f);
            heatmap.backgroundSprite = "GenericPanelWhite";

            UIPanel blank4 = heatmap.AddUIComponent<UIPanel>();
            blank4.size = new Vector2(328, 16);

            UIPanel row0 = heatmap.AddUIComponent<UIPanel>();
            row0.size = new Vector2(328, 8);
            UILabel minLabel = row0.AddUIComponent<UILabel>();
            minLabel.size = new Vector2(24, 8);
            minLabel.textScale = 0.5f;
            minLabel.text = "Min";
            minLabel.textColor = Color.black;
            minLabel.relativePosition = new Vector3(18, 0);
            UILabel lsLabel = row0.AddUIComponent<UILabel>();
            lsLabel.size = new Vector2(64, 8);
            lsLabel.textScale = 0.5f;
            lsLabel.text = "linewidth/colour";
            lsLabel.textColor = Color.black;
            lsLabel.relativePosition = new Vector3(44, 0);
            UILabel cutLabel = row0.AddUIComponent<UILabel>();
            cutLabel.size = new Vector2(64, 8);
            cutLabel.textScale = 0.5f;
            cutLabel.text = "journeys";
            cutLabel.textColor = Color.black;
            cutLabel.relativePosition = new Vector3(140, 0);

            UIPanel[] rows = new UIPanel[9];

            for (int idx = 1; idx < 9; idx++)
            {
                UIPanel row1 = rows[idx];
                row1 = heatmap.AddUIComponent<UIPanel>();
                row1.size = new Vector2(328f, 16f);
                UICheckBox minChk = row1.AddUIComponent<UICheckBox>();
                minChk.name = "minChk" + idx;
                minChk.size = new Vector2(12, 12f);
                minChk.relativePosition = new Vector3(18, 2);
                _minChks[idx] = minChk;
                minChk.eventClicked += new MouseEventHandler(MinChkClick);
                UISprite checkoff = minChk.AddUIComponent<UISprite>();
                checkoff.spriteName = "check-unchecked";
                checkoff.size = new Vector2(12f, 12f);
                checkoff.relativePosition = new Vector3(0, 1);
                minChk.checkedBoxObject = (UIComponent)checkoff.AddUIComponent<UISprite>();
                ((UISprite)minChk.checkedBoxObject).spriteName = "check-checked";
                minChk.checkedBoxObject.size = new Vector2(12f, 12f);
                minChk.checkedBoxObject.relativePosition = Vector3.zero;
                UILabel label = row1.AddUIComponent<UILabel>();
                label.size = new Vector2(12f, 12f);
                label.textColor = Color.black;
                label.textScale = 0.75f;
                label.text = "" + idx;
                label.relativePosition = new Vector2(42, 4);
                UISprite cp = row1.AddUIComponent<UISprite>();
                cp.spriteName = "ColorPickerColor";
                cp.size = new Vector2(64f, idx * 2);
                cp.color = JVutils.cutoffsColor[idx];
                cp.relativePosition = new Vector2(66f, 8 - idx);
                UITextField cutpoint = row1.AddUIComponent<UITextField>();
                cutpoint.name = "cutpoint" + idx;
                cutpoint.size = new Vector2(36f, 12f);
                cutpoint.textScale = 0.75f;
                cutpoint.textColor = Color.black;
                if (idx == 8)
                {
                    cutpoint.text = "" + JVutils.cutoffs[8];
                    UILabel plus = row1.AddUIComponent<UILabel>();
                    plus.size = new Vector2(12, 12);
                    plus.text = "+";
                    plus.textColor = Color.black;
                    plus.textScale = 0.75f;
                    plus.relativePosition = new Vector2(186, 4);
                }
                else
                {
                    cutpoint.text = "" + JVutils.cutoffs[idx];
                }
                cutpoint.horizontalAlignment = UIHorizontalAlignment.Right;
                cutpoint.verticalAlignment = UIVerticalAlignment.Bottom;
                cutpoint.relativePosition = new Vector2(148, 4);
                //cutpoint.maxLength = 5;
                cutpoint.builtinKeyNavigation = true;
                cutpoint.submitOnFocusLost = true;
                cutpoint.focusedBgSprite = "TextFieldPanel";
                cutpoint.hoveredBgSprite = "TextFieldPanelHovered";
                cutpoint.selectionSprite = "EmptySprite";
                _cutBoxes[idx] = cutpoint;
                cutpoint.eventTextSubmitted += new PropertyChangedEventHandler<string>(CutpointChanged);

                if (idx == 4)
                {
                    UIButton doubleBtn = UIUtils.CreateButton((UIComponent)row1);
                    doubleBtn.textColor = bblue;
                    doubleBtn.textScale = 0.625f;
                    doubleBtn.font = ptChk.label.font;
                    doubleBtn.size = new Vector2(100f, 12f);
                    doubleBtn.relativePosition = new Vector3(200, 1);
                    doubleBtn.eventClick += new MouseEventHandler(DoubleBtn);
                    doubleBtn.text = "Double cutoffs";

                }
                if (idx == 5)
                {
                    UIButton halveBtn = UIUtils.CreateButton((UIComponent)row1);
                    halveBtn.textColor = bblue;
                    halveBtn.textScale = 0.625f;
                    halveBtn.font = ptChk.label.font;
                    halveBtn.size = new Vector2(100f, 12f);
                    halveBtn.relativePosition = new Vector3(200, 1);
                    halveBtn.eventClick += new MouseEventHandler(HalveBtn);
                    halveBtn.text = "Halve cutoffs";

                }


            }

            UIPanel blank5 = heatmap.AddUIComponent<UIPanel>();
            blank5.size = new Vector2(328, 8);

            UIPanel cpanel = heatmap.AddUIComponent<UIPanel>();
            cpanel.size = new Vector2(328, 18);
            //UILabel blank6 = cpanel.AddUIComponent<UILabel>();
            //blank6.size = new Vector2(24, 18);
            UIButton colouring = UIUtils.CreateButton((UIComponent)cpanel);
            colouring.name = "HeatSwitcher";
            colouring.textColor = bblue;
            colouring.textScale = 11f / 16f;
            colouring.size = new Vector2(280f, 18f);
            colouring.relativePosition = new Vector3(24, 0);
            colouring.eventClick += new MouseEventHandler(HeatToggle);
            if (theJV.HeatMap)
                colouring.text = "Switch to line colours";
            else
                colouring.text = "Switch to heatmap colours";
            _colouringBtn = colouring;

            UIPanel blendspace = uiPanel2.AddUIComponent<UIPanel>();
            blendspace.size = new Vector2(328, 12);

            UIPanel blend = uiPanel2.AddUIComponent<UIPanel>();
            blend.name = "PTstretches";
            blend.anchor = (UIAnchorStyle)13;
            blend.autoLayout = true;
            blend.autoLayoutDirection = (LayoutDirection)0;
            blend.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            blend.autoLayoutStart = (LayoutStart)0;
            blend.size = new Vector2(328f, 20f);
            UICheckBox blendChk = blend.AddUIComponent<UICheckBox>();
            blendChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            blendChk.clipChildren = true;
            blendChk.eventClicked += new MouseEventHandler(BlendChkClick);
            UISprite blendSprite = blendChk.AddUIComponent<UISprite>();
            blendSprite.spriteName = "check-unchecked";
            blendSprite.size = new Vector2(16f, 16f);
            blendSprite.relativePosition = Vector3.zero;
            blendChk.checkedBoxObject = (UIComponent)blendSprite.AddUIComponent<UISprite>();
            ((UISprite)blendChk.checkedBoxObject).spriteName = "check-checked";
            blendChk.checkedBoxObject.size = new Vector2(16f, 16f);
            blendChk.checkedBoxObject.relativePosition = Vector3.zero;
            blendChk.label = blendChk.AddUIComponent<UILabel>();
            //blendChk.label.font = UIUtils.Font;
            blendChk.label.textColor = bblue;
            blendChk.label.disabledTextColor = (Color32)Color.black;
            blendChk.label.textScale = 0.8125f;
            blendChk.label.text = "blend lines on same lane";
            blendChk.label.relativePosition = new Vector3(22f, 2f);
            blendChk.size = new Vector2(blendChk.label.width + 22f, 16f);
            _blendChk = blendChk;

            UIPanel stops = uiPanel2.AddUIComponent<UIPanel>();
            stops.name = "PTstretches";
            stops.anchor = (UIAnchorStyle)13;
            stops.autoLayout = true;
            stops.autoLayoutDirection = (LayoutDirection)0;
            stops.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            stops.autoLayoutStart = (LayoutStart)0;
            stops.size = new Vector2(328f, 20f);
            UICheckBox stopsChk = stops.AddUIComponent<UICheckBox>();
            stopsChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            stopsChk.clipChildren = true;
            stopsChk.eventClicked += new MouseEventHandler(StopsChkClick);
            UISprite stopsSprite = stopsChk.AddUIComponent<UISprite>();
            stopsSprite.spriteName = "check-unchecked";
            stopsSprite.size = new Vector2(16f, 16f);
            stopsSprite.relativePosition = Vector3.zero;
            stopsChk.checkedBoxObject = (UIComponent)stopsSprite.AddUIComponent<UISprite>();
            ((UISprite)stopsChk.checkedBoxObject).spriteName = "check-checked";
            stopsChk.checkedBoxObject.size = new Vector2(16f, 16f);
            stopsChk.checkedBoxObject.relativePosition = Vector3.zero;
            stopsChk.label = stopsChk.AddUIComponent<UILabel>();
            //stopsChk.label.font = UIUtils.Font;
            stopsChk.label.textColor = bblue;
            stopsChk.label.disabledTextColor = (Color32)Color.black;
            stopsChk.label.textScale = 0.8125f;
            stopsChk.label.text = "show PT stops on lines";
            stopsChk.label.relativePosition = new Vector3(22f, 2f);
            stopsChk.size = new Vector2(stopsChk.label.width + 22f, 16f);
            _stopsChk = stopsChk;

            UIPanel cars = uiPanel2.AddUIComponent<UIPanel>();
            cars.name = "PTstretches";
            cars.anchor = (UIAnchorStyle)13;
            cars.autoLayout = true;
            cars.autoLayoutDirection = (LayoutDirection)0;
            cars.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            cars.autoLayoutStart = (LayoutStart)0;
            cars.size = new Vector2(328f, 20f);
            UICheckBox carsChk = cars.AddUIComponent<UICheckBox>();
            carsChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            carsChk.clipChildren = true;
            carsChk.eventClicked += new MouseEventHandler(CarsChkClick);
            UISprite carsSprite = carsChk.AddUIComponent<UISprite>();
            carsSprite.spriteName = "check-unchecked";
            carsSprite.size = new Vector2(16f, 16f);
            carsSprite.relativePosition = Vector3.zero;
            carsChk.checkedBoxObject = (UIComponent)carsSprite.AddUIComponent<UISprite>();
            ((UISprite)carsChk.checkedBoxObject).spriteName = "check-checked";
            carsChk.checkedBoxObject.size = new Vector2(16f, 16f);
            carsChk.checkedBoxObject.relativePosition = Vector3.zero;
            carsChk.label = carsChk.AddUIComponent<UILabel>();
            //carsChk.label.font = UIUtils.Font;
            carsChk.label.textColor = bblue;
            carsChk.label.disabledTextColor = (Color32)Color.black;
            carsChk.label.textScale = 0.8125f;
            carsChk.label.text = "include all car journeys";
            carsChk.label.relativePosition = new Vector3(22f, 2f);
            carsChk.size = new Vector2(carsChk.label.width + 22f, 16f);
            _carsChk = carsChk;

            UIButton journeysBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            journeysBtn.textColor = bblue;
            journeysBtn.textScale = 0.75f;
            journeysBtn.size = new Vector2(328f, 20f);
            journeysBtn.eventClick += new MouseEventHandler(JourneysBtn);
            journeysBtn.text = "click through individual journeys";

            UIButton allBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            allBtn.textColor = bblue;
            allBtn.textScale = 0.75f;
            allBtn.size = new Vector2(328f, 20f);
            allBtn.eventClick += new MouseEventHandler(AllBtn);
            allBtn.text = "show all journeys (slow!)";
            allBtn.color = Color.green;

            UIPanel max = uiPanel2.AddUIComponent<UIPanel>();
            max.size = new Vector2(328, 20);
            UILabel maxlabel = max.AddUIComponent<UILabel>();
            maxlabel.size = new Vector2(100f, 12f);
            maxlabel.textColor = bblue;
            maxlabel.textScale = 0.75f;
            maxlabel.text = "Max journeys:";
            maxlabel.relativePosition = Vector3.zero;
            UITextField maxtext = max.AddUIComponent<UITextField>();
            maxtext.name = "maxtext";
            maxtext.size = new Vector2(44f, 12f);
            maxtext.textScale = 0.75f;
            maxtext.textColor = bblue;
            maxtext.text = "" + theJV.m_maxJourneysCount;
            maxtext.horizontalAlignment = UIHorizontalAlignment.Right;
            maxtext.relativePosition = new Vector2(106, 0);
            maxtext.maxLength = 5;
            maxtext.builtinKeyNavigation = true;
            maxtext.submitOnFocusLost = true;
            maxtext.focusedBgSprite = "TextFieldPanel";
            maxtext.hoveredBgSprite = "TextFieldPanelHovered";
            maxtext.selectionSprite = "EmptySprite";
            _maxtext = maxtext;
            maxtext.eventTextSubmitted += new PropertyChangedEventHandler<string>(MaxtextChanged);

            UIPanel exitspace = uiPanel2.AddUIComponent<UIPanel>();
            exitspace.size = new Vector2(328, 24);

            UIButton exitBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            exitBtn.textColor = bblue;
            exitBtn.textScale = 0.75f;
            exitBtn.size = new Vector2(328f, 20f);
            exitBtn.eventClick += new MouseEventHandler(ExitBtn);
            exitBtn.text = "exit from Journeys viewer";
            exitBtn.color = Color.red;

            Debug.Log("Completed SetupPanel code");
        }

        private void FromChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.FromToFlag == 1)
                theJV.FromToFlag = 0;
            else
            {
                if (theJV.FromToFlag == 2)
                    _toChk.isChecked = false;
                theJV.FromToFlag = 1;
            }
            theJV.ShowJourneys();
        }
        private void ToChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.FromToFlag == 2)
                theJV.FromToFlag = 0;
            else
            {
                if (theJV.FromToFlag == 1)
                    _fromChk.isChecked = false;
                theJV.FromToFlag = 2;
            }
            theJV.ShowJourneys();
        }
        private void ResiChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.TouristFlag == 1)
                theJV.TouristFlag = 0;
            else
            {
                if (theJV.TouristFlag == 2)
                    _tourChk.isChecked = false;
                theJV.TouristFlag = 1;
            }
            theJV.ShowJourneys();
        }
        private void TourChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.TouristFlag == 2)
                theJV.TouristFlag = 0;
            else
            {
                if (theJV.TouristFlag == 1)
                    _resiChk.isChecked = false;
                theJV.TouristFlag = 2;
            }
            theJV.ShowJourneys();
        }
        private void PTstretchesToggled(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.ToggleShowPTstretches();
        }
        private void LaneCycleBtn(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.SubselectByLaneLine();
        }
        private void LaneCycleReverseBtn(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.SubselectByLaneLine(forwards: false);
        }
        private void PTLaneChkClick(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.ToggleTransportSteps();
        }

        private void SecondABBtn(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.MakeSecondarySelection)
            {
                theJV.MakeSecondarySelection = false;
                _modeABChk.Disable();
                _modeABChk.isChecked = false;
                _secondABBtn.text = "make a secondary selection";
            }
            else
            {
                theJV.MakeSecondarySelection = true;
                _modeABChk.Enable();
                _modeABChk.label.textColor = Color.red;
                _modeABChk.isChecked = true;
                _secondABBtn.text = "switch back to primary selection mode";
                theJV.ModeSecondarySelection = true;
            }
        }

        public void PanelRefreshMode()
        {
            _modeABChk.Disable();
            _modeABChk.isChecked = false;
            _secondABBtn.text = "make a secondary selection";
        }


        private void ModeABChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            theJV.ModeSecondarySelection = !theJV.ModeSecondarySelection;
        }

        private void PlusBtn(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.MakeExtendedSelection)
            {
                theJV.MakeExtendedSelection = false;
                _plusChk.Disable();
                _plusChk.isChecked = false;
                _plusBtn.text = "extend the selection";
            }
            else
            {
                theJV.MakeExtendedSelection  = true;
                _plusChk.Enable();
                _plusChk.label.textColor = Color.red;
                _plusChk.isChecked = true;
                _plusBtn.text = "switch back to primary selection mode";
                theJV.ModeExtendedSelection = true;
            }
        }

        public void PanelRefreshExtendMode()
        {
            _plusChk.Disable();
            _plusChk.isChecked = false;
            _plusBtn.text = "extend the selection";
        }


        private void PlusChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            theJV.ModeExtendedSelection = !theJV.ModeExtendedSelection;
        }

        private void MinChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            for (int idx = 1; idx < 9; idx++)
                _minChks[idx].isChecked = false;
            int ID = Convert.ToInt32(component.name.Substring(6));
            _minChks[ID].isChecked = true;
            theJV.MinHalfwidth = ID;
            theJV.CallReheat();
        }

        private void CutpointChanged(UIComponent component, string newName)
        {
            int ID = Convert.ToInt32(component.name.Substring(8));
            if (ID == 7 || ID == 8)
            {
                JVutils.cutoffs[7] = Convert.ToInt32(newName);
                JVutils.cutoffs[8] = Convert.ToInt32(newName);
                _cutBoxes[7].text = newName;
                _cutBoxes[8].text = newName;
            }
            else
            {
                JVutils.cutoffs[ID] = Convert.ToInt32(newName);
            }
            Singleton<JourneyVisualizer>.instance.CallReheat();
        }

        private void DoubleBtn(UIComponent component, UIMouseEventParameter p)
        {
            for (int idx = 1; idx < 9; idx++)
            {
                JVutils.cutoffs[idx] *= 2;
                _cutBoxes[idx].text = "" + JVutils.cutoffs[idx];
            }
            Singleton<JourneyVisualizer>.instance.CallReheat();
        }


        private void HalveBtn(UIComponent component, UIMouseEventParameter p)
        {
            for (int idx = 1; idx < 9; idx++)
            {
                JVutils.cutoffs[idx] /= 2;
                _cutBoxes[idx].text = "" + JVutils.cutoffs[idx];
            }
            Singleton<JourneyVisualizer>.instance.CallReheat();
        }

        private void HeatToggle(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.HeatMap)
                _colouringBtn.text = "Switch to heatmap colours";
            else
                _colouringBtn.text = "Switch to line colours";
            theJV.ChangeHeatMap();
        }

        private void BlendChkClick(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.ToggleShowBlended();
        }

        private void StopsChkClick(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.ToggleShowPTstops();
        }

        private void CarsChkClick(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.ToggleAllCars();
        }

        private void JourneysBtn(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.ByJourney();
        }
        private void AllBtn(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            theJV.ShowAllJourneys();
            _fromChk.isChecked = false;
            _toChk.isChecked = false;
            _ptChk.isChecked = false;
        }

        private void MaxtextChanged(UIComponent component, string newName)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            theJV.m_maxJourneysCount = Convert.ToInt32(newName);
            theJV.RefreshJourneys = true;
            theJV.SimulationStep(0);
            theJV.RefreshJourneys = false;
            theJV.ShowJourneys();           // to recreate per the settings for From To and PTstretches
            //_fromChk.isChecked = false;
            //_toChk.isChecked = false;
            //_ptChk.isChecked = false;
        }

        private Vector3 m_deltaPos;

        private void JPanelMouseMove(UIComponent component, UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = m_OwnerView.fixedHeight - mousePos.y;
                _JPanel.absolutePosition = mousePos + m_deltaPos;
            }
        }
        private void JPanelMouseDown(UIComponent component, UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = m_OwnerView.fixedHeight - mousePos.y;
                m_deltaPos = _JPanel.absolutePosition - mousePos;
                _JPanel.BringToFront();
            }
        }

        private void JPanelDblClick(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneysButton>.instance._JButton.absolutePosition = new Vector3(180, 18);
        }

        private void ExitBtn(UIComponent component, UIMouseEventParameter p)
        {
            if (Redirector<JourneyDetourer>.IsDeployed())
            {
                Redirector<JourneyDetourer>.Revert();
                Singleton<JourneysButton>.instance.m_inJourneys = false;
                _JPanel.Hide();
                if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes)
                {
                    UIView.library.Show("TrafficRoutesInfoViewPanel");
                    Singleton<NetManager>.instance.PathVisualizer.PathsVisible = true;
                }
            }
        }

    }

    public class JourneysButton : UIPanel
    {
        private UIPanel _JPanel;
        public UIPanel _JButton;
        public bool m_inJourneys;
        public static JourneysButton instance;


        public override void Awake()
        {
            base.Awake();
            instance = this;
            SetupButton();
            m_inJourneys = false;
            m_moveInitialized = false;
            m_deltaPos = Vector3.zero;
            _JPanel = Singleton<JourneysPanel>.instance._JPanel;
            _JButton.Show();
            //_JPanel.Show();
            //Show();
            Debug.Log("done button awake");
        }



        public void SetupButton()
        {
            //relativePosition = new Vector3(130f, 120f);
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
            //jButton.relativePosition = jOffset;
            //jButton.eventMouseDown += new MouseEventHandler(JButtonMouseDown);
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
            Debug.Log("done SetUpButton");
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
                }
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
        //private void JButtonMouseDown(UIComponent component, UIMouseEventParameter p)
        //{
        //    if (p.buttons.IsFlagSet(UIMouseButton.Right))
        //    {
        //        Vector3 mousePos = Input.mousePosition;
        //        mousePos.y = m_OwnerView.fixedHeight - mousePos.y;
        //        Debug.Log("MDown - abs pos x: " + _JButton.absolutePosition.x + " y: " + _JButton.absolutePosition.y);
        //        Debug.Log("MDown - mouse pos x: " + mousePos.x + " y: " + mousePos.y);
        //        m_deltaPos = _JButton.absolutePosition - mousePos;
        //        Debug.Log("MDown - delta x: " + m_deltaPos.x + " y: " + m_deltaPos.y);
        //        _JButton.BringToFront();
        //    }
        //}

        //protected override void OnMouseMove(UIMouseEventParameter p)
        //{
        //    if (p.buttons.IsFlagSet(UIMouseButton.Right))
        //    {
        //        Vector3 mousePos = Input.mousePosition;
        //        mousePos.y = m_OwnerView.fixedHeight - mousePos.y;

        //        _JButton.absolutePosition = mousePos + m_deltaPos;
        //    }
        //}
        //protected override void OnMouseMove(UIMouseEventParameter p)
        //{
        //    if (p.buttons.IsFlagSet(UIMouseButton.Right))
        //    {
        //        Vector3 mousePos = Input.mousePosition;
        //        mousePos.y = m_OwnerView.fixedHeight - mousePos.y;

        //        _JButton.absolutePosition = mousePos + m_deltaPos;
        //    }
        //}
        //private Vector3 m_deltaPos;
        //protected override void OnMouseDown(UIMouseEventParameter p)
        //{
        //    if (p.buttons.IsFlagSet(UIMouseButton.Right))
        //    {
        //        Vector3 mousePos = Input.mousePosition;
        //        mousePos.y = m_OwnerView.fixedHeight - mousePos.y;

        //        m_deltaPos = _JButton.absolutePosition - mousePos;
        //        BringToFront();
        //    }
        //}

        //protected override void OnMouseMove(UIMouseEventParameter p)
        //{
        //    if (p.buttons.IsFlagSet(UIMouseButton.Right))
        //    {
        //        Vector3 mousePos = Input.mousePosition;
        //        mousePos.y = m_OwnerView.fixedHeight - mousePos.y;

        //        _JButton.absolutePosition = mousePos + m_deltaPos;
        //    }
        //}
    }
    //public void Init()
    //{
    //    Debug.Log("Called JP Init");
    //    //JourneysPanel instance = this;
    //    SetupPanel();
    //    Show();
    //    //m_label = Find<UILabel>("");
    //    //m_Strip = Find<UITabstrip>("Tabstrip");
    //    //m_CitizensContainer = Find("Citizens").Find("Container");
    //    //m_LinesContainer = Find("JourneyLines").Find("Container");
    //}

    //public override void Show()
    //{
    //    this.Show();
    //    Debug.Log("finished Show");
    //}

}
