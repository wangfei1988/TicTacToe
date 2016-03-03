using System;
using System.Collections.Generic;

namespace IntelliMedia
{
	public delegate void CompletedHandler(object result);
	public delegate void ActionHandler(AsyncTask previousTask, CompletedHandler onComplete, ErrorHandler onError);
	public delegate void ErrorHandler(Exception e);
	public delegate void FinallyHandler();

	public class AsyncTask
	{
		private ActionHandler onAction;
		private CompletedHandler onCompleted;
		private ErrorHandler onError;
		private FinallyHandler onFinally;

		private AsyncTask parent;
		private List<AsyncTask> nextTasks = new List<AsyncTask>();

		public object Result { get; private set; }

		public AsyncTask(ActionHandler actionHandler)
		{
			onAction = actionHandler;
		}			

		public T ResultAs<T>()
		{
			if (Result is T)
			{
				return (T)Result;
			}
			else
			{
				throw new Exception(String.Format("Attempting to cast AsyncTask result '{0}' to '{1}'", 
					(Result != null ? Result.GetType().Name : "null"),
					typeof(T).Name));				
			}
		}

		public AsyncTask Then(ActionHandler actionHandler)
		{
			AsyncTask next = new AsyncTask(actionHandler);
			next.parent = this;
			nextTasks.Add(next);

			return next;
		}

		public AsyncTask Catch(ErrorHandler errorHandler)
		{
			if (errorHandler != null) 
			{
				onError += errorHandler;
			}

			return this;
		}

		public AsyncTask Finally(FinallyHandler action)
		{
			if (action != null) 
			{
				onFinally += action;
			}

			return this;
		}

		public void Start(CompletedHandler completedHandler = null, ErrorHandler errorHandler = null)
		{
			AsyncTask rootTask = this;
			while (rootTask.parent != null) 
			{
				rootTask = rootTask.parent;
			}

			if (rootTask == this) 
			{
				rootTask.ExecuteTask (null, completedHandler, errorHandler);
			}
			else 
			{
				if (completedHandler != null) {
					this.onCompleted += completedHandler;
				}
				if (errorHandler != null) {
					this.onError += errorHandler;
				}
				rootTask.ExecuteTask (null, null, null);
			}
		}

		private void ExecuteTask(AsyncTask prevResult = null, CompletedHandler completedHandler = null, ErrorHandler errorHandler = null)
		{
			if (completedHandler != null) 
			{
				onCompleted += completedHandler;
			}

			if (errorHandler != null) 
			{
				onError += errorHandler;
			}

			try
			{
				onAction(prevResult, Completed, Error);
			}
			catch(Exception e) 
			{
				Error(e);
			}
		}

		private void Completed(object result)
		{
			if (onCompleted != null) 
			{
				onCompleted(result);
			}

			Result = result;
			if (Result is AsyncTask) 
			{
				nextTasks.Insert(0, (AsyncTask)Result);
			}

			foreach (AsyncTask task in nextTasks) 
			{
				task.ExecuteTask(this, null, null);
			}

			if (onFinally != null) 
			{
				onFinally();
			}
		}

		private void Error(Exception e)
		{
			if (onError != null) 
			{
				onError (e);
			}

			foreach (AsyncTask task in nextTasks) 
			{				
				task.Error(e);
			}	

			if (onFinally != null) 
			{
				onFinally();
			}
		}
	}
}

