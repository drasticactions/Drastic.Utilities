// <copyright file="BaseViewModel.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Drastic.ViewModels
{
    /// <summary>
    /// Base View Model.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool isBusy;
        private string title = string.Empty;
        private string isLoadingText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceProvider"/>.</param>
        public BaseViewModel(IServiceProvider services)
        {
            this.Services = services;
            this.Dispatcher = services.GetService(typeof(IAppDispatcher)) as IAppDispatcher ?? throw new NullReferenceException(nameof(IAppDispatcher));
            this.ErrorHandler = services.GetService(typeof(IErrorHandlerService)) as IErrorHandlerService ?? throw new NullReferenceException(nameof(IErrorHandlerService));
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets a baseline navigation handler.
        /// Handle this to handle navigation events within the view model.
        /// </summary>
        public event EventHandler<NavigationEventArgs>? Navigation;

        /// <summary>
        /// Gets or sets a value indicating whether the VM is busy.
        /// </summary>
        public bool IsBusy
        {
            get { return this.isBusy; }
            set { this.SetProperty(ref this.isBusy, value); }
        }

        /// <summary>
        /// Gets or sets the is loading text.
        /// </summary>
        public string IsLoadingText
        {
            get { return this.isLoadingText; }
            set { this.SetProperty(ref this.isLoadingText, value); }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get { return this.title; }
            set { this.SetProperty(ref this.title, value); }
        }

        /// <summary>
        /// Gets the Error Handler.
        /// </summary>
        public IErrorHandlerService ErrorHandler { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Gets the Dispatcher.
        /// </summary>
        public IAppDispatcher Dispatcher { get; }

        /// <summary>
        /// Sets title for page.
        /// </summary>
        /// <param name="title">The Title.</param>
        public virtual void SetTitle(string title = "")
        {
            this.Title = title;
        }

        /// <summary>
        /// Called on VM Load.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        public virtual Task OnLoad()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends a navigation request to whatever handlers attach to it.
        /// </summary>
        /// <param name="viewModel">The view model type.</param>
        /// <param name="arguments">Arguments to send to the view model.</param>
        public void SendNavigationRequest(Type viewModel, object? arguments = default)
        {
            if (viewModel.IsSubclassOf(typeof(BaseViewModel)))
            {
                this.Navigation?.Invoke(this, new NavigationEventArgs(viewModel, arguments));
            }
        }

        /// <summary>
        /// Performs an Async task while setting the <see cref="IsBusy"/> variable.
        /// If the task throws, it is handled by <see cref="ErrorHandler"/>.
        /// </summary>
        /// <param name="action">Task to run.</param>
        /// <param name="isLoadingText">Optional Is Loading text.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task PerformBusyAsyncTask(Func<Task> action, string isLoadingText = "", CancellationToken? cancellationToken = default)
        {
            this.IsLoadingText = isLoadingText;
            this.IsBusy = true;

            try
            {
                // Start the action task
                var actionTask = action.Invoke();
                var token = cancellationToken ?? CancellationToken.None;

                // Create a task that completes when the cancellation token is triggered
                var cancellationTask = Task.Delay(Timeout.Infinite, token);

                // Await the first task to complete: either the action task or the cancellation task
                var completedTask = await Task.WhenAny(actionTask, cancellationTask);

                if (completedTask == cancellationTask)
                {
                    // If the cancellation task completed first, throw an OperationCanceledException
                    throw new OperationCanceledException(token);
                }

                // Await the action task to propagate any exceptions
                await actionTask;
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation specifically if needed
                // Optionally log or notify about the cancellation
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                this.ErrorHandler.HandleError(ex);
            }
            finally
            {
                // Ensure that these properties are reset even if an exception occurs
                this.IsBusy = false;
                this.IsLoadingText = string.Empty;
            }
        }

        /// <summary>
        /// Called when wanting to raise a Command Can Execute.
        /// </summary>
        public virtual void RaiseCanExecuteChanged()
        {
        }

#pragma warning disable SA1600 // Elements should be documented
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.Dispatcher?.Dispatch(() =>
            {
                var changed = this.PropertyChanged;
                if (changed == null)
                {
                    return;
                }

                changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
