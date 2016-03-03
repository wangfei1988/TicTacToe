//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Center for Educational Informatics
// http://www.cei.ncsu.edu/
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this 
//     list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//---------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;

namespace IntelliMedia
{
	public class MainMenuViewModel : ViewModel
	{
		private StageManager navigator;
		private SessionState sessionState;
		private AuthenticationService authenticator;
		private ActivityService activityService;
		private ActivityLauncher activityLauncher;

		public string Username { get; set; }

		public readonly BindableProperty<List<Activity>> ActivitiesProperty = new BindableProperty<List<Activity>>();	
		public List<Activity> Activities 
		{ 
			get { return ActivitiesProperty.Value; }
			set { ActivitiesProperty.Value = value; }
		}

		public readonly BindableProperty<List<ActivityState>> ActivityStatesProperty = new BindableProperty<List<ActivityState>>();	
		public List<ActivityState> ActivityStates 
		{ 
			get { return ActivityStatesProperty.Value; }
			set { ActivityStatesProperty.Value = value; }
		}
		
		public MainMenuViewModel(
			StageManager navigator, 
			SessionState sessionState,
			AuthenticationService authenticator, 
			ActivityService activityService,
			ActivityLauncher activityLauncher)
		{
			this.navigator = navigator;
			this.sessionState = sessionState;
			this.authenticator = authenticator;
			this.activityService = activityService;
			this.activityLauncher = activityLauncher;
		}

		public override void OnStartReveal()
		{
			RefreshActivityList();
			base.OnStartReveal ();
		}

		private void RefreshActivityList()
		{
			try
			{
				Contract.PropertyNotNull("sessionState.CourseSettings", sessionState.CourseSettings);
				
				DebugLog.Info("RefreshActivityList");
				navigator.Reveal<ProgressIndicatorViewModel>().Then((vm, onRevealed, onRevealError) =>
				{
					ProgressIndicatorViewModel progressIndicatorViewModel = vm.ResultAs<ProgressIndicatorViewModel>();
                    ProgressIndicatorViewModel.ProgressInfo busyIndicator = progressIndicatorViewModel.Begin("Loading...");
                    activityService.LoadActivities(sessionState.CourseSettings.CourseId)
					.Then((prevResult, onCompleted, onError) =>
                    {
						DebugLog.Info("Activities loaded");	
						Activities = prevResult.ResultAs<List<Activity>>();
						IEnumerable<string> activityIds = Activities.Select(a => a.Id);
						activityService.LoadActivityStates(sessionState.Student.Id, activityIds).Start(onCompleted, onError);
                    })
					.Then((prevResult, onCompleted, onError) =>
                    {
						DebugLog.Info("Activity States loaded");	
						ActivityStates = prevResult.ResultAs<List<ActivityState>>();
						onCompleted(true);
                    })
                    .Catch((Exception e) =>
                    {
						DebugLog.Error("Can't load activitues: {0}", e.Message);	
                        navigator.Reveal<AlertViewModel>(alert =>
                        {
                             alert.Title = "Unable to load activity information.";
                             alert.Message = e.Message;
                             alert.Error = e;
                             alert.AlertDismissed += ((int index) => DebugLog.Info("Button {0} pressed", index));
						}).Start();

                    }).Finally(() =>
                    {
                        busyIndicator.Dispose();
					}).Start();

					onRevealed(true);
				}).Start();
			}
			catch (Exception e)
			{
				navigator.Reveal<AlertViewModel>(alert => 
				{
					alert.Title = "Unable load activity information";
					alert.Message = e.Message;
					alert.Error = e;
				}).Start();
			}
		}

		public void StartActivity(Activity activity)
		{
			try
			{
				Contract.ArgumentNotNull("activity", activity);
				
				DebugLog.Info("Started Activity {0}", activity.Name);

				navigator.Reveal<ProgressIndicatorViewModel>().Then((vm, onRevealed, onRevealError) =>
				{
					ProgressIndicatorViewModel progressIndicatorViewModel = vm.ResultAs<ProgressIndicatorViewModel>();
                    ProgressIndicatorViewModel.ProgressInfo busyIndicator = progressIndicatorViewModel.Begin("Starting...");
                    activityLauncher.Start(sessionState.Student, activity, false)
					.Then((prevResult, onCompleted, onError) =>
                    {
						navigator.Transition(this, prevResult.ResultAs<ActivityViewModel>());
						onCompleted(true);
                    })
                    .Catch((Exception e) =>
                   {
                       navigator.Reveal<AlertViewModel>(alert =>
                        {
                            alert.Title = "Unable to start activity";
                            alert.Message = e.Message;
                            alert.Error = e;
                            alert.AlertDismissed += ((int index) => DebugLog.Info("Button {0} pressed", index));
						}).Start();

                   }).Finally(() =>
                   {
                       busyIndicator.Dispose();
					}).Start();
				}).Start();
			}
			catch (Exception e)
			{
				navigator.Reveal<AlertViewModel>(alert => 
				                                 {
					alert.Title = "Unable to start activity";
					alert.Message = e.Message;
					alert.Error = e;
				}).Start();
			}
		}

		public void SignOut()
		{
			DebugLog.Info("SignOut");
			try
			{
				authenticator.SignOut((bool success, string message) => 
				{
					navigator.Transition(this, typeof(SignInViewModel));
				});
			}
			catch (Exception e)
			{
				navigator.Reveal<AlertViewModel>(alert => 
				                                 {
					alert.Title = "Unable to sign out";
					alert.Message = e.Message;
					alert.Error = e;
					alert.AlertDismissed += ((int index) => DebugLog.Info("Button {0} pressed", index));
				}).Start();
			}
		}

		private void LoadActivities()
		{

		}
	}
}
