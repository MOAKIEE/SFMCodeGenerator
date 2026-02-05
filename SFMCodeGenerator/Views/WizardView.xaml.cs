namespace SFMCodeGenerator.Views
{
    public partial class WizardView : System.Windows.Controls.UserControl
    {
        public WizardView()
        {
            InitializeComponent();
            
            // 订阅属性变化以更新代码
            if (DataContext is ViewModels.WizardViewModel vm)
            {
                vm.SubscribeToChanges();
            }
        }
    }
}
