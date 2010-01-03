﻿using System;


namespace Dfo.Login
{
	[Serializable]
	public class DfoAuthenticationException : Exception
	{
		public DfoAuthenticationException() { }
		public DfoAuthenticationException( string message ) : base( message ) { }
		public DfoAuthenticationException( string message, Exception inner ) : base( message, inner ) { }
		protected DfoAuthenticationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[Serializable]
	public class DfoLaunchException : Exception
	{
		public DfoLaunchException() { }
		public DfoLaunchException( string message ) : base( message ) { }
		public DfoLaunchException( string message, Exception inner ) : base( message, inner ) { }
		protected DfoLaunchException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}
}

/*
 Copyright 2009 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/