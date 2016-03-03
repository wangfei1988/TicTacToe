﻿//---------------------------------------------------------------------------------------
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
using System.Collections;

namespace IntelliMedia
{
	public class SessionService 
	{
		private AppSettings appSettings;

		public SessionService(AppSettings appSettings)
		{
			this.appSettings = appSettings;
		}
		
		public AsyncTask Start(string sessionId)
		{
			return new AsyncTask((prevResult, onCompleted, onError) =>
			{
				try
				{
					Uri serverUri = new Uri(appSettings.ServerURI, UriKind.RelativeOrAbsolute);
					Uri restUri = new Uri(serverUri, "rest/");

					SessionRepository repo = new SessionRepository(restUri);
					if (repo == null)
					{
						throw new Exception("SessionRepository is not initialized.");
					}
								
					repo.GetByKey(sessionId, (response) =>
					{
						if (response.Success)
						{
							onCompleted(response.Item);
						}
						else
						{
							onError(new Exception(response.Error));
						}
					});                   
				}
				catch (Exception e)
				{
					onError(e);
				}
			});
		}
	}
}
