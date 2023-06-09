using ColossalFramework;
using ColossalFramework.UI;
using System;
using UnityEngine;
using Journeys.RedirectionFramework;


namespace Journeys
{

    public class JourneysPanel : UIPanel
    {
        public static JourneysPanel instance;
        private readonly JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
        private UILabel _numSelectedLabel;
        private UILabel _numSubselectedLabel;
        private UICheckBox _fromChk;
        private UICheckBox _toChk;
        private UICheckBox _ptChk;
        private UICheckBox _resiChk;
        private UICheckBox _tourChk;
        private UICheckBox _ptLaneChk;
        private UIButton _lanelineBtn;
        private UIButton _reverseBtn;
        private UIButton _secondABBtn;
        private UICheckBox _secondChk;
        private UIButton _plusBtn;
        private UITextField[] _cutBoxes;
        private UICheckBox[] _minChks;
        private UIButton _colouringBtn;
        private UICheckBox _blendChk;
        private UICheckBox _stopsChk;
        private UICheckBox _carsChk;
        public UIPanel _JPanel;

        public override void Awake()
        {

            Debug.Log("JV: started jPanel awake");
            base.Awake();
            instance = this;
            _minChks = new UICheckBox[9];
            _cutBoxes = new UITextField[9];
            SetupPanel();
            UpdateBothSelected(0, 0);
            _toChk.isChecked = false;
            _fromChk.isChecked = false;
            _ptChk.isChecked = false;
            _ptLaneChk.isChecked = true;
            _resiChk.isChecked = false;
            _tourChk.isChecked = false;
            _minChks[1].isChecked = true;
            _secondChk.isChecked = false;
            _blendChk.isChecked = false;
            _stopsChk.isChecked = false;
            _carsChk.isChecked = false;
            for (int idx = 1; idx < 9; idx++)
            {
                _cutBoxes[idx].text = "" + JVutils.cutoffs[idx];
            }
            _JPanel.Hide();
            Debug.Log("JV: done jPanel awake");
        }

        public void SetupPanel()
        {
            Color32 bblue = new Color32(185, 221, 254, 255);

            size = Vector2.zero;
            absolutePosition = new Vector3(-1000, -1000);  //otherwise there is a dead area lower-mid-screen
            UIPanel uiPanel0 = AddUIComponent<UIPanel>();
            uiPanel0.canFocus = true;
            uiPanel0.isInteractive = true;
            uiPanel0.anchor = UIAnchorStyle.None;
            uiPanel0.pivot = UIPivotPoint.TopLeft;
            uiPanel0.width = 360f;
            uiPanel0.height = 800f;
            uiPanel0.absolutePosition = new Vector3(80f, 58f);
            uiPanel0.backgroundSprite = "SubcategoriesPanel";
            uiPanel0.eventMouseDown += new MouseEventHandler(JPanelMouseDown);
            uiPanel0.eventMouseMove += new MouseEventHandler(JPanelMouseMove);
            uiPanel0.eventDoubleClick += new MouseEventHandler(JPanelDblClick);
            _JPanel = uiPanel0;
            UIPanel uiPanel1 = uiPanel0.AddUIComponent<UIPanel>();
            uiPanel1.height = 40f;
            uiPanel1.relativePosition = Vector3.zero;
            UILabel uiTitle = uiPanel1.AddUIComponent<UILabel>();
            uiTitle.height = 25f;
            uiTitle.width = 200f;
            uiTitle.padding = new RectOffset(0, 0, 4, 0);
            uiTitle.textAlignment = UIHorizontalAlignment.Center;
            uiTitle.relativePosition = new Vector3(110, 7.5f);

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
            uiPanel3.anchor = (UIAnchorStyle)13;
            uiPanel3.autoLayout = true;
            uiPanel3.autoLayoutDirection = (LayoutDirection)0;
            uiPanel3.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel3.autoLayoutStart = (LayoutStart)0;
            uiPanel3.size = new Vector2(328f, 18f);
            UILabel numselected = uiPanel3.AddUIComponent<UILabel>();
            numselected.autoSize = true;
            numselected.height = 18f;
            numselected.textColor = bblue;
            numselected.font = UIUtils.Font;
            _numSelectedLabel = numselected;

            UIPanel uiPanel3a = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel3a.size = new Vector2(328, 9);

            UIPanel uiPanel4 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel4.anchor = (UIAnchorStyle)13;
            uiPanel4.autoLayout = true;
            uiPanel4.autoLayoutDirection = (LayoutDirection)0;
            uiPanel4.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel4.autoLayoutStart = (LayoutStart)0;
            uiPanel4.size = new Vector2(328f, 18f);
            UILabel numsubselected = uiPanel4.AddUIComponent<UILabel>();
            numsubselected.autoSize = true;
            numsubselected.height = 18f;
            numsubselected.textColor = bblue;
            numsubselected.font = UIUtils.Font;
            _numSubselectedLabel = numsubselected;

            UIPanel uiPanel4a = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel4a.size = new Vector2(328, 12);

            UIPanel uiPanel5 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel5.anchor = (UIAnchorStyle)13;
            uiPanel5.autoLayout = true;
            uiPanel5.autoLayoutDirection = (LayoutDirection)0;
            uiPanel5.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel5.autoLayoutStart = (LayoutStart)0;
            uiPanel5.size = new Vector2(328f, 20f);
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
            fromChk.label.textColor = bblue;
            fromChk.label.disabledTextColor = (Color32)Color.black;
            fromChk.label.textScale = 13f / 16f;
            fromChk.label.text = "show only From here";
            fromChk.label.relativePosition = new Vector3(22f, 2f);
            fromChk.size = new Vector2(fromChk.label.width + 22f, 16f);
            _fromChk = fromChk;
            UICheckBox toChk = uiPanel5.AddUIComponent<UICheckBox>();
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
            ptChk.label.textColor = bblue;
            ptChk.label.disabledTextColor = (Color32)Color.black;
            ptChk.label.textScale = 13f / 16f;
            ptChk.label.text = "show only PT stretches and transfers";
            ptChk.label.relativePosition = new Vector3(22f, 2f);
            ptChk.size = new Vector2(ptChk.label.width + 22f, 16f);
            _ptChk = ptChk;

            UIPanel tourism = uiPanel2.AddUIComponent<UIPanel>();
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
            uiPanel7.anchor = (UIAnchorStyle)13;
            uiPanel7.autoLayout = true;
            uiPanel7.autoLayoutDirection = (LayoutDirection)0;
            uiPanel7.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel7.autoLayoutStart = (LayoutStart)0;
            uiPanel7.size = new Vector2(328f, 18f);
            uiPanel7.useCenter = true;
            UIButton uiButton1 = UIUtils.CreateButton((UIComponent)uiPanel7);
            uiButton1.textColor = bblue;
            uiButton1.textScale = 11f / 16f;
            uiButton1.font = ptChk.label.font;
            uiButton1.size = new Vector2(180f, 18f);
            uiButton1.relativePosition = Vector3.zero;
            uiButton1.eventClick += new MouseEventHandler(LaneCycleBtn);
            uiButton1.text = "Cycle through lane/line";
            _lanelineBtn = uiButton1;
            UIButton uiButton2 = UIUtils.CreateButton((UIComponent)uiPanel7);
            uiButton2.font = ptChk.label.font;
            uiButton2.name = "ReverseLaneLine";
            uiButton2.textColor = bblue;
            uiButton2.textScale = 11f / 16f;
            uiButton2.size = new Vector2(140f, 18f);
            uiButton2.relativePosition = new Vector2(260f, 0f);
            uiButton2.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            uiButton2.eventClick += new MouseEventHandler(LaneCycleReverseBtn);
            uiButton2.text = "Reverse cycle";
            _reverseBtn = uiButton2;

            UIPanel uiPanel8 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel8.anchor = (UIAnchorStyle)13;
            uiPanel8.autoLayout = true;
            uiPanel8.autoLayoutDirection = (LayoutDirection)0;
            uiPanel8.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel8.autoLayoutStart = (LayoutStart)0;
            uiPanel8.size = new Vector2(328f, 16f);

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
            secondABBtn.text = "make a secondary subselection";
            _secondABBtn = secondABBtn;

            UIPanel uiPanel9 = uiPanel2.AddUIComponent<UIPanel>();
            uiPanel9.anchor = (UIAnchorStyle)13;
            uiPanel9.autoLayout = true;
            uiPanel9.autoLayoutDirection = (LayoutDirection)0;
            uiPanel9.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            uiPanel9.autoLayoutStart = (LayoutStart)0;
            uiPanel9.size = new Vector2(328f, 16f);

            UILabel blankAB2 = uiPanel9.AddUIComponent<UILabel>();
            blankAB2.size = new Vector2(32, 16);

            UICheckBox secondChk = uiPanel9.AddUIComponent<UICheckBox>();
            secondChk.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            secondChk.clipChildren = true;
            secondChk.eventClicked += new MouseEventHandler(SecondChkClick);
            UISprite UISpriteAB2 = secondChk.AddUIComponent<UISprite>();
            UISpriteAB2.spriteName = "check-unchecked";
            UISpriteAB2.size = new Vector2(16f, 16f);
            UISpriteAB2.relativePosition = Vector3.zero;
            secondChk.checkedBoxObject = (UIComponent)UISpriteAB2.AddUIComponent<UISprite>();
            ((UISprite)secondChk.checkedBoxObject).spriteName = "check-checked";
            secondChk.checkedBoxObject.size = new Vector2(16f, 16f);
            secondChk.checkedBoxObject.relativePosition = Vector3.zero;
            secondChk.label = secondChk.AddUIComponent<UILabel>();
            secondChk.label.textColor = bblue;
            secondChk.label.disabledTextColor = (Color32)Color.black;
            secondChk.label.textScale = 0.75f;
            secondChk.label.text = "make further secondary selections";
            secondChk.label.relativePosition = new Vector3(22f, 2f);
            secondChk.size = new Vector2(secondChk.label.width + 22f, 16f);
            _secondChk = secondChk;

            UIPanel plusBlank = uiPanel2.AddUIComponent<UIPanel>();
            plusBlank.size = new Vector2(328, 12);

            UIButton plusBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            plusBtn.textColor = bblue;
            plusBtn.textScale = 0.75f;
            plusBtn.size = new Vector2(328f, 18f);
            plusBtn.eventClick += new MouseEventHandler(PlusBtn);
            plusBtn.text = "extend the primary selection";
            _plusBtn = plusBtn;

            UIPanel blank3 = uiPanel2.AddUIComponent<UIPanel>();
            blank3.size = new Vector2(328, 16);

            UIPanel heatmap = uiPanel2.AddUIComponent<UIPanel>();
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
            UIButton colouring = UIUtils.CreateButton((UIComponent)cpanel);
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
            stopsChk.label.textColor = bblue;
            stopsChk.label.disabledTextColor = (Color32)Color.black;
            stopsChk.label.textScale = 0.8125f;
            stopsChk.label.text = "show PT stops on lines";
            stopsChk.label.relativePosition = new Vector3(22f, 2f);
            stopsChk.size = new Vector2(stopsChk.label.width + 22f, 16f);
            _stopsChk = stopsChk;

            UIPanel cars = uiPanel2.AddUIComponent<UIPanel>();
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
            carsChk.label.textColor = bblue;
            carsChk.label.disabledTextColor = (Color32)Color.black;
            carsChk.label.textScale = 0.8125f;
            carsChk.label.text = "include all car journeys";
            carsChk.label.relativePosition = new Vector3(22f, 2f);
            carsChk.size = new Vector2(carsChk.label.width + 22f, 16f);
            _carsChk = carsChk;

            //UIButton journeysBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            //journeysBtn.textColor = bblue;
            //journeysBtn.textScale = 0.75f;
            //journeysBtn.size = new Vector2(328f, 20f);
            //journeysBtn.eventClick += new MouseEventHandler(JourneysBtn);
            //journeysBtn.text = "click through individual journeys";

            UIPanel journeys = uiPanel2.AddUIComponent<UIPanel>();
            journeys.anchor = (UIAnchorStyle)13;
            journeys.autoLayout = true;
            journeys.autoLayoutDirection = (LayoutDirection)0;
            journeys.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
            journeys.autoLayoutStart = (LayoutStart)0;
            journeys.size = new Vector2(328f, 18f);
            journeys.useCenter = true;
            UIButton journeysBtn1 = UIUtils.CreateButton((UIComponent)journeys);
            journeysBtn1.textColor = bblue;
            journeysBtn1.textScale = 11f / 16f;
            journeysBtn1.font = ptChk.label.font;
            journeysBtn1.size = new Vector2(180f, 18f);
            journeysBtn1.relativePosition = Vector3.zero;
            journeysBtn1.eventClick += new MouseEventHandler(JourneysBtn);
            journeysBtn1.text = "Click through journeys";
            UIButton journeysBtn2 = UIUtils.CreateButton((UIComponent)journeys);
            journeysBtn2.font = ptChk.label.font;
            journeysBtn2.textColor = bblue;
            journeysBtn2.textScale = 11f / 16f;
            journeysBtn2.size = new Vector2(140f, 18f);
            journeysBtn2.relativePosition = new Vector2(260f, 0f);
            journeysBtn2.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            journeysBtn2.eventClick += new MouseEventHandler(JourneysReverseBtn);
            journeysBtn2.text = "Reverse cycle";

            UIButton allBtn = UIUtils.CreateButton((UIComponent)uiPanel2);
            allBtn.textColor = bblue;
            allBtn.textScale = 0.75f;
            allBtn.size = new Vector2(328f, 20f);
            allBtn.eventClick += new MouseEventHandler(AllBtn);
            allBtn.text = "show every known journey (no max - slow!)";
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
            maxtext.text = "" + theJV.MaxJourneysCount;
            maxtext.horizontalAlignment = UIHorizontalAlignment.Right;
            maxtext.relativePosition = new Vector2(106, 0);
            maxtext.maxLength = 5;
            maxtext.builtinKeyNavigation = true;
            maxtext.submitOnFocusLost = true;
            maxtext.focusedBgSprite = "TextFieldPanel";
            maxtext.hoveredBgSprite = "TextFieldPanelHovered";
            maxtext.selectionSprite = "EmptySprite";
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
        }

        public void UpdateBothSelected(int numselected, int numsubselected, string jqualifier = " selected")
        {
            string js = numselected == 1 ? " journey" : " journeys";
            _numSelectedLabel.text = numselected + js + jqualifier;
            js = numsubselected == 1 ? " journey" : " journeys";
            _numSubselectedLabel.text = numsubselected + js + " in subselection";
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
            theJV.ShowJourneys(theJV.m_selectedCims);
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
            theJV.ShowJourneys(theJV.m_selectedCims);
        }

        private void FromToDisable()
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            theJV.FromToFlag = 0;
            _fromChk.isChecked = false;
            _toChk.isChecked = false;
            _fromChk.Disable();
            _toChk.Disable();
        }

        private void FromToEnable()
        {
            _fromChk.Enable();
            _toChk.Enable();
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
            theJV.ShowJourneys(theJV.m_selectedCims);
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
            theJV.ShowJourneys(theJV.m_selectedCims);
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

        public void LaneLineDisable()
        {
            _lanelineBtn.Disable();
            _reverseBtn.Disable();
            _ptLaneChk.isChecked = false;       // set it false else it stands out blue looking weird. It's reset to JV flag on Enable.
            _ptLaneChk.Disable();
        }
        public void LaneLineEnable()
        {
            _lanelineBtn.Enable();
            _reverseBtn.Enable();
            _ptLaneChk.isChecked = Singleton<JourneyVisualizer>.instance.ShowOnlyTransportLanes;
            _ptLaneChk.Enable();
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
                theJV.ModeAdditionalSecondarySelection = false;
                theJV.RestoreSelection();
                _secondABBtn.Unfocus();                            // we unfocus it so the following color and text updates take place immediately
                _secondABBtn.textColor = new Color32(185, 221, 254, 255);
                _secondABBtn.text = "make a secondary selection";
                _secondChk.Unfocus();
                _secondChk.isChecked = false;
                _secondChk.Disable();
                _plusBtn.Enable();
            }
            else
            {
                theJV.MakeSecondarySelection = true;
                theJV.MakeExtendedSelection = false;
                _plusBtn.Unfocus();
                _plusBtn.textColor = new Color32(185, 221, 254, 255);
                _plusBtn.text = "extend the primary selection";
                _plusBtn.Disable();
                _secondABBtn.Unfocus();
                _secondABBtn.textColor = Color.red;
                _secondABBtn.text = "switch back to primary selection mode";
                _secondChk.Enable();
            }
        }

        private void SecondChkClick(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            theJV.ModeAdditionalSecondarySelection = !theJV.ModeAdditionalSecondarySelection;
            // according to mode just selected, save or restore m_selectedCims to what it was when there were only primary selections (ready for a new green target)
            theJV.SaveOrRestoreSelection();
        }

        public void AfterAll()
        {
            _secondABBtn.Enable();
            _plusBtn.Enable();
            FromToEnable();
        }

        public void PanelRefreshMode()
        {
            _secondABBtn.textColor = new Color32(185, 221, 254, 255);
            _secondABBtn.text = "make a secondary selection";
        }


        private void PlusBtn(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            if (theJV.MakeExtendedSelection)
            {
                theJV.MakeExtendedSelection = false;
                _plusBtn.Unfocus();
                _plusBtn.textColor = new Color32(185, 221, 254, 255);
                _plusBtn.text = "extend the primary selection";
            }
            else
            {
                theJV.MakeExtendedSelection = true;
                _plusBtn.Unfocus();
                _plusBtn.textColor = Color.red;
                _plusBtn.text = "switch back to primary selection mode";
            }
        }

        public void PanelRefreshExtendMode()
        {
            _plusBtn.textColor = new Color32(185, 221, 254, 255);
            _plusBtn.text = "extend the primary selection";
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
            theJV.ToggleHeatMap();
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
        private void JourneysReverseBtn(UIComponent component, UIMouseEventParameter p)
        {
            Singleton<JourneyVisualizer>.instance.ByJourney(forwards: false);
        }

        private void AllBtn(UIComponent component, UIMouseEventParameter p)
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            FromToDisable();
            _ptChk.isChecked = false;
            _resiChk.isChecked = false;
            _tourChk.isChecked = false;
            _secondABBtn.Disable();
            _secondChk.Disable();
            _plusBtn.Disable();
            theJV.ShowAllJourneys();
        }

        // I orignally had this work "live" by re-running the siumulation step, but that will only work for a single primary selection
        private void MaxtextChanged(UIComponent component, string newName)
        {
            Singleton<JourneyVisualizer>.instance.MaxJourneysCount = Convert.ToInt32(newName);
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
                Singleton<JourneysButton>.instance._jbutton.textColor = Color.red;
                Singleton<JourneysButton>.instance._jbutton.Unfocus();
            }
        }

    }

