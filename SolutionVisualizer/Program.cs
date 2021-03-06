﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using lib;
using lib.ProjectionSolver;
using SquareConstructor;
using Path = System.IO.Path;

namespace SolutionVisalizer
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var form = new VisualizerForm();
			Application.Run(form);
		}
		
	}
}