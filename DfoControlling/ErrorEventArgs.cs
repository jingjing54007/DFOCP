﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.Controlling
{
	/// <summary>
	/// Notifies an event listener of an error.
	/// </summary>
	public class ErrorEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the exception that caused the error.
		/// </summary>
		public Exception Error { get; private set; }

		/// <summary>
		/// Constructs a new <c>ErrorEventArgs</c> instance.
		/// </summary>
		/// <param name="error">The exception that caused the error.</param>
		public ErrorEventArgs( Exception error )
		{
			Error = error;
		}
	}
}

/*
 Copyright 2010 Greg Najda

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