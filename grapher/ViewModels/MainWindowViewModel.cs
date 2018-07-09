﻿using grapher.Helpers;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace grapher.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private DiagramViewModel _DiagramViewModel;
        private ObservableCollection<RenderItemViewModel> _RenderItems;
        private List<SelectableDesignerItemViewModelBase> _itemsToRemove;
        private ToolBoxViewModel _ToolBoxViewModel;
        private ToolBarViewModel _ToolBarViewModel;

        public InteractionRequest<Notification> OpenColorPickerRequest { get; } = new InteractionRequest<Notification>();

        public InteractionRequest<Notification> OpenFillColorPickerRequest { get; } = new InteractionRequest<Notification>();

        public MainWindowViewModel()
        {
            DiagramViewModel = new DiagramViewModel();
            ToolBoxViewModel = new ToolBoxViewModel();
            ToolBarViewModel = new ToolBarViewModel();

            DeleteSelectedItemsCommand = new DelegateCommand<object>(p =>
            {
                ExecuteDeleteSelectedItemsCommand(p);
            });
            SelectColorCommand = new DelegateCommand<DiagramViewModel>(p =>
            {
                var exchange = new ColorExchange()
                {
                    Old = DiagramViewModel.EdgeColors.FirstOrDefault()
                };
                OpenColorPickerRequest.Raise(new Notification() { Title = "Color picker", Content = exchange });
                if (exchange.New.HasValue)
                {
                    DiagramViewModel.EdgeColors.Clear();
                    DiagramViewModel.EdgeColors.Add(exchange.New.Value);
                    foreach (var item in DiagramViewModel.SelectedItems.OfType<DesignerItemViewModelBase>())
                    {
                        item.EdgeColor = exchange.New.Value;
                    }
                    foreach (var item in DiagramViewModel.SelectedItems.OfType<ConnectorBaseViewModel>())
                    {
                        item.EdgeColor = exchange.New.Value;
                    }
                }
            });
            SelectFillColorCommand = new DelegateCommand<DiagramViewModel>(p =>
            {
                var exchange = new ColorExchange()
                {
                    Old = DiagramViewModel.FillColors.FirstOrDefault()
                };
                OpenFillColorPickerRequest.Raise(new Notification() { Title = "Color picker", Content = exchange });
                if (exchange.New.HasValue)
                {
                    DiagramViewModel.FillColors.Clear();
                    DiagramViewModel.FillColors.Add(exchange.New.Value);
                    foreach (var item in DiagramViewModel.SelectedItems.OfType<DesignerItemViewModelBase>())
                    {
                        item.FillColor = exchange.New.Value;
                    }
                }
            });
        }

        public DiagramViewModel DiagramViewModel
        {
            get { return _DiagramViewModel; }
            set { SetProperty(ref _DiagramViewModel, value); }
        }

        public ToolBoxViewModel ToolBoxViewModel
        {
            get { return _ToolBoxViewModel; }
            set { SetProperty(ref _ToolBoxViewModel, value); }
        }

        public ToolBarViewModel ToolBarViewModel
        {
            get { return _ToolBarViewModel; }
            set { SetProperty(ref _ToolBarViewModel, value); }
        }

        public ReactiveProperty<string> CurrentOperation { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> Details { get; } = new ReactiveProperty<string>();

        public DelegateCommand<object> DeleteSelectedItemsCommand { get; private set; }

        public DelegateCommand<DiagramViewModel> SelectColorCommand { get; }

        public DelegateCommand<DiagramViewModel> SelectFillColorCommand { get; }

        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            _itemsToRemove = DiagramViewModel.SelectedItems.ToList();
            List<SelectableDesignerItemViewModelBase> connectionsToAlsoRemove = new List<SelectableDesignerItemViewModelBase>();

            foreach (var connector in DiagramViewModel.Items.OfType<ConnectorBaseViewModel>())
            {
                if (connector.SourceConnectorInfo is FullyCreatedConnectorInfo
                    && ItemsToDeleteHasConnector(_itemsToRemove, connector.SourceConnectorInfo as FullyCreatedConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }

                if (connector.SinkConnectorInfo is FullyCreatedConnectorInfo
                    && ItemsToDeleteHasConnector(_itemsToRemove, connector.SinkConnectorInfo as FullyCreatedConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }
            }
            _itemsToRemove.AddRange(connectionsToAlsoRemove);
            foreach (var selectedItem in _itemsToRemove)
            {
                DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
            }
        }

        private bool ItemsToDeleteHasConnector(List<SelectableDesignerItemViewModelBase> itemsToRemove, FullyCreatedConnectorInfo connector)
        {
            return itemsToRemove.Contains(connector.DataItem);
        }
    }
}
