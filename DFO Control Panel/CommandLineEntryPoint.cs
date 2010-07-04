﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Dfo.Controlling;
using System.Threading;

namespace Dfo.ControlPanel
{
	class CommandLineEntryPoint : IDisposable
	{
		private CommandLineArgs m_parsedArgs;
		private DfoLauncher m_launcher = new DfoLauncher();
		private AutoResetEvent m_stateBecameNoneEvent = new AutoResetEvent( false );

		public CommandLineEntryPoint( CommandLineArgs parsedArgs )
		{
			m_parsedArgs = parsedArgs;
		}

		public int Run()
		{
			if ( m_parsedArgs.ShowVersion )
			{
				Console.WriteLine( "{0} version {1}", VersionInfo.AssemblyTitle, VersionInfo.AssemblyVersion );
				Console.WriteLine( VersionInfo.AssemblyCopyright );
				Console.WriteLine( VersionInfo.LicenseStatement );
			}
			if ( m_parsedArgs.ShowHelp )
			{
				CommandLineArgs.DisplayHelp( Console.Out );
			}

			if ( m_parsedArgs.ShowVersion || m_parsedArgs.ShowHelp )
			{
				return 0;
			}

			bool invalidArgs = false;

			if ( m_parsedArgs.Settings.Username == null )
			{
				Logging.Log.FatalFormat( "You must supply a username." );
				invalidArgs = true;
			}

			if ( m_parsedArgs.Settings.Password == null )
			{
				Logging.Log.FatalFormat( "You must supply a password." );
				invalidArgs = true;
			}

			if ( invalidArgs )
			{
				Console.WriteLine( "Try {0} --help for usage information.", CommandLineArgs.GetProgramName() );
				return 1;
			}

			if ( m_parsedArgs.Settings.ClosePopup != null ) m_launcher.Params.ClosePopup = m_parsedArgs.Settings.ClosePopup.Value;
			//if ( m_parsedArgs.Settings.CustomSoundpackDir != null ) m_launcher.Params.CustomSoundpackDirRaw = m_parsedArgs.Settings.CustomSoundpackDir;
			if ( m_parsedArgs.Settings.DfoDir != null ) m_launcher.Params.GameDir = m_parsedArgs.Settings.DfoDir;
			if ( m_parsedArgs.Settings.LaunchWindowed != null ) m_launcher.Params.LaunchInWindowed = m_parsedArgs.Settings.LaunchWindowed.Value;
			if ( m_parsedArgs.Settings.Password != null ) m_launcher.Params.Password = m_parsedArgs.Settings.Password;
			//if ( m_parsedArgs.Settings.SwitchSoundpacks != null ) m_launcher.Params.SwitchSoundpacks = m_parsedArgs.Settings.SwitchSoundpacks.Value;
			//if ( m_parsedArgs.Settings.TempSoundpackDir != null ) m_launcher.Params.TempSoundpackDirRaw = m_parsedArgs.Settings.TempSoundpackDir;
			if ( m_parsedArgs.Settings.Username != null ) m_launcher.Params.Username = m_parsedArgs.Settings.Username;

			if ( m_parsedArgs.Settings.DfoDir == null )
			{
				try
				{
					m_launcher.Params.AutoDetectGameDir();
				}
				catch ( IOException ex )
				{
					Logging.Log.ErrorFormat( "Could not autodetect the DFO directory. {0}", ex.Message );
				}
			}

			foreach ( ISwitchableFile switchableFile in m_parsedArgs.Settings.SwitchableFiles.Values )
			{
				switchableFile.RelativeRoot = m_launcher.Params.GameDir;
				switchableFile.ApplyDefaults();

				if ( m_parsedArgs.Settings.SwitchFile[ switchableFile.Name ].HasValue &&
				   m_parsedArgs.Settings.SwitchFile[ switchableFile.Name ].Value )
				{
					FileSwitcher fileToSwitch = new FileSwitcher();
					fileToSwitch.FileToSwitch = switchableFile.ResolveNormalFile();
					fileToSwitch.FileToSwitchWith = switchableFile.ResolveCustomFile();
					fileToSwitch.TempFile = switchableFile.ResolveTempFile();

					m_launcher.Params.FilesToSwitch.Add( fileToSwitch );
				}
			}

			try
			{
				FixSwitchableFilesIfNeeded( m_parsedArgs.Settings.SwitchableFiles.Values );
			}
			catch ( IOException ex )
			{
				Logging.Log.FatalFormat(
					"Error while trying to fix switchable file. {0} I guess you'll have to fix it yourself.",
					ex.Message );

				return 2;
			}

			m_launcher.LaunchStateChanged += StateChangedHandler;
			m_launcher.FileSwitchFailed += FileSwitchFailHandler;
			m_launcher.WindowModeFailed += WindowFailHandler;
			m_launcher.PopupKillFailed += PopupKillFailHandler;

			Console.WriteLine( "You must leave this program running. It will automatically stop when the game is closed." );

			try
			{
				Logging.Log.DebugFormat( "Launching. Launch parameters:{0}{1}",
					Environment.NewLine, m_launcher.Params );
				m_launcher.Launch();

				Logging.Log.DebugFormat( "Waiting for state to become None." );
				m_stateBecameNoneEvent.WaitOne();

				Logging.Log.DebugFormat( "Done." );
				Console.WriteLine( "Done." );
			}
			catch ( Exception ex )
			{
				if ( ex is System.Security.SecurityException
				  || ex is System.Net.WebException
				  || ex is DfoAuthenticationException
				  || ex is DfoLaunchException )
				{
					Logging.Log.FatalFormat( "There was a problem while trying to start the game. {0}", ex.Message );
					return 2;
				}
				else
				{
					throw;
				}
			}

			return 0;
		}

		/// <summary>
		/// Fixes any switchable files in the given collection that are in an inconsistent state.
		/// </summary>
		/// <param name="filesToFix"></param>
		/// <exception cref="System.IO.IOException">Something went wrong while trying to fix a switchable file.</exception>
		private void FixSwitchableFilesIfNeeded( ICollection<SwitchableFile> filesToFix )
		{
			foreach ( SwitchableFile file in filesToFix )
			{
				bool wasBroken;
				file.FixBrokenFilesIfNeeded( out wasBroken );
				if ( wasBroken )
				{
					Console.WriteLine( "Switchable file {0} was detected to be in a mixed-up state (this is usually caused by a system crash). It has been fixed.",
						file.NormalFile );
				}
			}
		}

		///// <summary>
		///// Fixes any switchable files that are in an inconsistent state.
		///// </summary>
		///// <param name="switchableFiles"></param>
		///// <exception cref="System.IO.IOException">Something went wrong while trying to fix a switchable file.</exception>
		//private void FixSwitchableFiles( ICollection<FileSwitcher> switchableFiles )
		//{
		//    foreach ( FileSwitcher switchableFile in switchableFiles )
		//    {
		//        FixSwitchableFiles( switchableFile );
		//    }
		//}

		///// <summary>
		///// Attempts to fix a mixed-up switchable file usually caused by a system crash while the game is running.
		///// </summary>
		///// <param name="switchableFile">The file to check. Assumed to be already validated.</param>
		///// <exception cref="System.IO.IOException">Something went wrong while trying to fix the switchable file.</exception>
		//private void FixSwitchableFile( FileSwitcher switchableFile )
		//{
		//    Logging.Log.InfoFormat( "Checking integrity of switchable file {0}.", switchableFile.FileToSwitch );
		//    if ( m_launcher.SwitchableFilesBroken( switchableFile ) )
		//    {
		//        Logging.Log.Info( "Mixed up files detected, attempting to fix them..." );

		//        m_launcher.FixBrokenSwitchableFiles( switchableFile );

		//        Console.WriteLine( "Switchable file {0} was detected to be in a mixed-up state (this is usually caused by a system crash). It has been fixed.",
		//            switchableFile.FileToSwitch );
		//        Logging.Log.Info( "Fixed." );
		//    }
		//    else
		//    {
		//        Logging.Log.InfoFormat( "{0} is ok.", switchableFile.FileToSwitch );
		//    }
		//}

		///// <summary>
		///// Attempts to fix mixed-up soundpacks usually caused by a system crash while the game is running.
		///// This function uses m_launcher's DfoDir, CustomSoundpackDir, and TempSoundpackDir properties.
		///// </summary>
		///// <exception cref="System.IO.IOException">Something went wrong while trying to fix the soundpacks.</exception>
		//private void FixSoundpacksIfNeeded()
		//{
		//    // XXX: Code and message overlap with GUI code
		//    Logging.Log.Info( "Checking for broken soundpacks..." );
		//    if ( m_launcher.SoundpacksBroken() )
		//    {
		//        Logging.Log.Info( "Broken soundpack directories detected, attempting to fix them..." );

		//        m_launcher.FixBrokenSoundpacks();

		//        string successMessage = "Your soundpack directories were detected to be mixed up (this is usually caused by a system crash). They have been fixed.";
		//        Console.WriteLine( successMessage );
		//        Logging.Log.Info( successMessage );
		//    }
		//    else
		//    {
		//        Logging.Log.Info( "Soundpacks are OK." );
		//    }
		//}

		private void StateChangedHandler( object sender, EventArgs e )
		{
			Logging.Log.DebugFormat( "State change to {0}.", m_launcher.State );

			switch ( m_launcher.State )
			{
				case LaunchState.None:
					m_stateBecameNoneEvent.Set();
					break;
				case LaunchState.Login:
					Console.WriteLine( "Logging in..." );
					break;
				case LaunchState.Launching:
					Console.WriteLine( "Launching..." );
					break;
				case LaunchState.GameInProgress:
					Console.WriteLine( "Game up." );
					break;
			}
		}

		private void FileSwitchFailHandler( object sender, Dfo.Controlling.ErrorEventArgs e )
		{
			Logging.Log.ErrorFormat( "Switching files failed. {0}", e.Error.Message );
		}

		private void WindowFailHandler( object sender, CancelErrorEventArgs e )
		{
			Logging.Log.ErrorFormat( "The window mode setting could not be applied. {0}", e.Error.Message );
			e.Cancel = true;
		}

		private void PopupKillFailHandler( object sender, Dfo.Controlling.ErrorEventArgs e )
		{
			Logging.Log.ErrorFormat( "Could not kill the popup. {0}", e.Error.Message );
		}

		public void Dispose()
		{
			m_launcher.Dispose();
			m_stateBecameNoneEvent.Close();
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