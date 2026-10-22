using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Extensions;
using DevExpress.Data.Filtering;
using DevExpress.DevAV.ViewModels;
using DevExpress.Xpf.Core;

namespace DevExpress.DevAV.AiTools {
    public static class DevAvAITools {
        [AIIntegrationTool]
        [Description(
            "Navigates to module and optionally applies a filter. Use this tool for any navigation or filtering request. Avaiable module names: Employees, Customers, Products, Sales.")]
        public static async Task<object> NavigateToModuleAndFilter(
            [AIIntegrationToolTarget("Application global view model for navigation.")]
            DevAVDbViewModel viewModel,
            [Description(
                "Module name to be selected. Matches any words about potential module names: Employees, Customers, Products, Sales.")]
            string moduleName,
            [Description("Prompt in natural language that describes the filtering criteria.")]
            string filterPrompt = "") {
            if(viewModel == null)
                throw new ArgumentException("DevAVDbViewModel is null.");
            if(string.IsNullOrEmpty(moduleName))
                throw new ArgumentException("The ModuleName is empty.");

            NavigateToModule(viewModel, moduleName);

            var result = await FilterViewModel(moduleName, filterPrompt);

            if(!result.success)
                throw new ArgumentException(result.responseText);

            return new {
                success = true,
                moduleName = moduleName,
                filterApplied = !string.IsNullOrEmpty(filterPrompt),
                filterExpression = filterPrompt
            };
        }

        static void NavigateToModule(DevAVDbViewModel viewModel, string moduleName) {
            var module = viewModel.Modules.FirstOrDefault(x =>
                x.ModuleTitle.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

            if(viewModel.SelectedModule != module)
                viewModel.SelectedModule = module;
        }

        [AIIntegrationTool]
        [Description("Adds a new product in the ProductCollectionViewModel view model.")]
        public static object AddProduct(
            [AIIntegrationToolTarget("Application global view model for navigation.")]
            DevAVDbViewModel viewModel,
            [Description("The product name to be added.")]
            string productName,
            [Description(
                "The product category to be added. Possible values: Automation, Monitors, Projectors, Televisions, VideoPlayers.")]
            string category,
            decimal cost,
            decimal salePrice,
            decimal retailPrice) {
            if(string.IsNullOrEmpty(category) || string.IsNullOrEmpty(productName))
                throw new ArgumentException("Category or Products cannot be empty.");

            var productViewModel =
                ServiceLocator.Instance.GetService(typeof(ProductCollectionViewModel)) as ProductCollectionViewModel;
            var unit = productViewModel.UnitOfWorkFactory.CreateUnitOfWork();
            NavigateToModule(viewModel, "Products");

            ProductCategory categoryValue;
            Enum.TryParse<ProductCategory>(category, out categoryValue);
            var product = new Product() {
                Name = productName,
                Category = categoryValue,
                Available = true,
                ProductionStart = DateTime.Now,
                Cost = cost,
                SalePrice = salePrice,
                RetailPrice = retailPrice,
            };
            unit.Products.Add(product);
            unit.SaveChanges();

            return new { success = true };
        }

        [AIIntegrationTool]
        [Description(
            "Gets information about available application themes. Always use this method before changing the theme.")]
        public static object GetThemesInfo() => new {
            CurrentTheme = ApplicationThemeHelper.ApplicationThemeName,
            AvailableThemes = GetAvailableThemes()
        };

        [AIIntegrationTool]
        [Description("Changes the application theme.")]
        public static object ChangeTheme(
            [Description("The theme name to be applied.")]
            string themeName) {
            var themes = GetAvailableThemes();
            if(!themes.Contains(themeName))
               throw new ArgumentException($"Theme '{themeName}' is not available.");

            ApplicationThemeHelper.ApplicationThemeName = themeName;
            return new { success = true, appliedTheme = themeName };
        }

        static List<string> GetAvailableThemes() => new List<string> {
            "Win11Dark", "Win11Light",
            "Win10Dark", "Win10Light",
            "Office2019Black", "Office2019Colorful", "Office2019HighContrast", "VS2019Blue", "VS2019Dark",
            "VS2019Light",
        };

        static async Task<(bool success, string responseText)> FilterViewModel(string moduleName, string filterPrompt) {
            var moduleViewModel = GetViewModel(moduleName);
            if(!string.IsNullOrEmpty(filterPrompt)) {
                var filterResult = await ConstructFilterString(filterPrompt, moduleViewModel.GetMetadata());
                if(!filterResult.success)
                    return new(false, filterResult.responseText);
                moduleViewModel.FilterString = filterResult.responseText;
                return new(true, filterResult.responseText);
            }

            moduleViewModel.FilterString = null;
            return new(true, string.Empty);
        }

        static async Task<(bool success, string responseText)> ConstructFilterString(string filterPrompt,
            IEnumerable<PromptToExpressionRequestBase.ColumnInfo> metadata) {
            var functions = DataAccess.ExpressionEditor.ExpressionEditorContextHelper.GetFunctions().ToRequestFunctionInfo();
            var request = new PromptToFilterRequest(filterPrompt, string.Empty, metadata, functions);
            var result =
                await AIExtensionsContainerDesktop.Default.PromptToExpressionAsync(request, CancellationToken.None);

            if(string.IsNullOrEmpty(result.Response))
                return (false, "Error: failed to generate a filter expression.");

            try {
                CriteriaOperator.Parse(result.Response);
                return (true, result.Response);
            }
            catch {
                return (false, result.Response);
            }
        }

        static ISupportFiltering GetViewModel(string moduleName) {
            return moduleName switch {
                "Employees" => ServiceLocator.Instance.GetService(typeof(EmployeeCollectionViewModel)) as
                    ISupportFiltering,
                "Customers" => ServiceLocator.Instance.GetService(typeof(CustomerCollectionViewModel)) as
                    ISupportFiltering,
                "Products" => ServiceLocator.Instance.GetService(typeof(ProductCollectionViewModel)) as
                    ISupportFiltering,
                "Sales" => ServiceLocator.Instance.GetService(typeof(OrderCollectionViewModel)) as ISupportFiltering,
                _ => null
            };
        }
    }
}
