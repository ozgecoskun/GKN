using System;
using System.IO;
using System.Windows.Forms;
using DevExpress.Mvvm.POCO;
using DevExpress.DevAV.ViewModels;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Navigation;
using System.Drawing;
using DevExpress.Utils.Gesture;
using DevExpress.Utils.TouchHelpers;
using DevExpress.Utils.Animation;
using DevExpress.XtraBars.Docking2010.Customization;
using DevExpress.Utils.Menu;
using DevExpress.Utils.Taskbar.Core;
using DevExpress.Utils.Taskbar;

namespace DevExpress.DevAV {
    public partial class MainForm : XtraForm, IMainModule, ISwipeGestureClient {
        MainViewModel viewModel;
        bool allowFlyoutPanel = true;
        bool allowTransition = true;
        public MainForm() {
            TaskbarHelper.InitDemoJumpList(TaskbarAssistant.Default, this);
            Program.MainForm = this;
            Icon = Program.AppIcon;
            ShowSplashScreen();
            InitializeComponent();
            PrepareUI();
            InitViewModel();
            DevExpress.Utils.About.UAlgo.Default.DoEventObject(DevExpress.Utils.About.UAlgo.kDemo, DevExpress.Utils.About.UAlgo.pWinForms, this);
        }

        void ShowSplashScreen() {
            DevExpress.XtraSplashScreen.SplashScreenManager.ShowForm(this, typeof(XtraSplashScreen.DemoSplashScreen), true, true);
            XtraSplashScreen.SplashScreenManager.Default.SendCommand(XtraSplashScreen.DemoSplashScreenBase.SplashScreenCommand.UpdateLabelProductText, "DevExpress WinForms Controls");
            XtraSplashScreen.SplashScreenManager.Default.SendCommand(XtraSplashScreen.DemoSplashScreenBase.SplashScreenCommand.UpdateLabelDemoText, "When Only the Best Will Do");
        }
        void MainForm_Load(object sender, EventArgs e) {
            InitTileBar();
            mainTileBar.SelectedItem = employeesTileBarItem;
            viewModel.SelectModule(ModuleType.Employees);
            DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm();
        }
        void InitViewModel() {
            viewModel = ViewModelSource.Create(() => new MainViewModel(this));
            PrefetchChildModules();
            viewModel.ModuleAdded += viewModel_ModuleAdded;
            viewModel.ModuleRemoved += viewModel_ModuleRemoved;
            viewModel.ModuleTransitionCompleted += viewModel_ModuleTransitionCompleted;
        }


        private void PrefetchChildModules() {
            if(System.Diagnostics.Debugger.IsAttached) return;
            viewModel.GetModule(ModuleType.Opportunities);
            viewModel.GetModule(ModuleType.Tasks);
            viewModel.GetModule(ModuleType.Products);
            viewModel.GetModule(ModuleType.CustomersModule);
            viewModel.GetModule(ModuleType.Dashboard);
            viewModel.GetModule(ModuleType.Sales);
        }
        void viewModel_ModuleAdded(object sender, EventArgs e) {
            var moduleControl = sender as Control;
            moduleControl.Dock = DockStyle.Fill;
            moduleControl.Size = modulesContainer.ClientSize;
            moduleControl.Parent = modulesContainer;
        }
        void viewModel_ModuleRemoved(object sender, EventArgs e) {
            var moduleControl = sender as Control;
            moduleControl.Parent = null;
        }
        void transitionManager1_BeforeTransitionStarts(ITransition transition, System.ComponentModel.CancelEventArgs e) {
            bottomPanelBase1.Enabled = true;
        }
        void transitionManager1_AfterTransitionEnds(ITransition transition, System.EventArgs e) {
            if(!IsHandleCreated) return;
            var method = new MethodInvoker(() => {
                bottomPanelBase1.Enabled = true;
                var moduleControl = viewModel.SelectedModule as DevExpress.DevAV.Modules.BaseModuleControl;
                if(moduleControl != null) moduleControl.OnTransitionCompleted();
            });
            if(InvokeRequired) BeginInvoke(method);
            else method();
        }

        void viewModel_ModuleTransitionCompleted(object sender, EventArgs e) {

        }
        void ChangeToSlideAnimation() {
            transitionManager1.Transitions.Clear();
            DevExpress.Utils.Animation.SlideTransition slide = new SlideTransition();
            slide.Parameters.FrameInterval = 5000;
            Transition transition = new Transition();
            transition.TransitionType = slide;
            transition.Control = modulesContainer;
            transitionManager1.Transitions.Add(transition);
        }
        void PrepareUI() {
            if(Program.IsTablet) {
                ChangeToSlideAnimation();
                TouchKeyboardSupport.EnableTouchKeyboard = true;
                SetupAsTablet();
                return;
            }
            SetupHeightWidth();
            DisableBottomPanelSwipe();
        }
        void SetupHeightWidth() {
            if(Screen.PrimaryScreen.WorkingArea.Height > 970) {
                ClientSize = new Size(ClientSize.Width, 945);
            }
        }
        void DisableBottomPanelSwipe() {
            bottomPanelBase1.Dock = DockStyle.Bottom;
            bottomPanelBase1.Parent = this;
            bottomPanelBase1.SendToBack();
            allowFlyoutPanel = false;
        }
        void SetupAsTablet() {
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            DisableBottomPanelSwipe();
            WindowsFormsSettings.PopupMenuStyle = XtraEditors.Controls.PopupMenuStyle.RadialMenu;
        }
        void InitTileBar() {
            employeesTileBarItem.Tag = ModuleType.Employees;
            employeesTileBarItem.Tag = ModuleType.Employees;
            customersTileBarItem.Tag = ModuleType.CustomersModule;
            tasksTileBarItem.Tag = ModuleType.Tasks;
            productsTileBarItem.Tag = ModuleType.Products;
            dashboardTileBarItem.Tag = ModuleType.Dashboard;
            salesTileBarItem.Tag = ModuleType.Sales;
            opportunitiesTileBarItem.Tag = ModuleType.Opportunities;
        }
        bool transitionEffective = false;
        public void StartTransition(bool effective) {
            this.transitionEffective = effective;
            if(!allowTransition) return;
            if(effective) transitionManager1.StartTransition(modulesContainer);
        }
        public void EndTransition(bool effective) {
            if(!effective || !allowTransition) {
                transitionManager1_AfterTransitionEnds(null, EventArgs.Empty);
                return;
            }
            transitionManager1.EndTransition();
        }
        public void OnSwipe(SwipeEventArgs args) {
            if(args.IsBottomEdge && allowFlyoutPanel) {
                flyoutPanel1.ShowPopup();
            }
        }
        public static void ShowNewItemMessage(Control source) {
            XtraMessageBox.Show(source.FindForm(), "Add NewItem", "Waring", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        void navButtonClose_ElementClick(object sender, NavElementEventArgs e) {
            Close();
        }
        public bool IsDocked(ModuleType type) { return true; }
        public void DockModule(ModuleType moduleType) { }
        public void ShowPeek(ModuleType moduleType) { }
        public void SaveLayoutToStream(MemoryStream ms) { }
        public void RestoreLayoutFromStream(MemoryStream ms) { }
        public IDXMenuManager MenuManager { get { return barManager1; } }

        private void productTileBar_ItemClick(object sender, TileItemEventArgs e) {
            mainTileBar.HideDropDownWindow();
        }

        private void customTileBar_ItemClick(object sender, TileItemEventArgs e) {
            mainTileBar.HideDropDownWindow();
        }

        private void mainTileBar_SelectedItemChanged(object sender, TileItemEventArgs e) {
            if(e.Item.Tag is ModuleType) {
                viewModel.SelectModule((ModuleType)e.Item.Tag);
            }
        }

        private void navButtonSettings_ElementClick(object sender, NavElementEventArgs e) {
            Modules.SettingsUC settingsUC = new Modules.SettingsUC(allowTransition);
            DialogResult result = FlyoutDialog.Show(this, settingsUC);
            if(result == System.Windows.Forms.DialogResult.OK) {
                allowTransition = settingsUC.checkEdit1.Checked;
            }
        }
        void navButtonHelp_ElementClick(object sender, NavElementEventArgs e) {
            DevExpress.Utils.About.AboutHelper.Show(DevExpress.Utils.About.ProductKind.DXperienceWin, new DevExpress.Utils.About.ProductStringInfo("Hybrid App", "WinForm Controls"));
        }
    }
}
