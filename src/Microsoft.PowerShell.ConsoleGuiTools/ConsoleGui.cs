// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using OutGridView.Models;
using Terminal.Gui;

namespace OutGridView.Cmdlet
{
    internal class ConsoleGui
    {
        private const string ACCEPT_TEXT = "Are you sure you want to select\nthese items to send down the pipeline?";
        private const string CANCEL_TEXT = "Are you sure you want to cancel?\nNothing will be emitted to the pipeline.";
        private const string CLOSE_TEXT = "Are you sure you want to close?";

        private bool _cancelled;

        internal HashSet<int> SelectedIndexes { get; private set; } = new HashSet<int>();
        public void Start(ApplicationData applicationData)
        {
            Application.Init();
            var top = Application.Top;

            // Creates the top-level window to show
            var win = new Window(applicationData.Title ?? "Out-ConsoleGridView")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu
                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem []
            {
                new MenuBarItem("_Options", 
                    applicationData.PassThru
                    ? new MenuItem []
                    {
                        new MenuItem("_Accept", "", () => { if (Quit("Accept", ACCEPT_TEXT)) top.Running = false; }),
                        new MenuItem("_Cancel", "", () =>{ if (Quit("Cancel", CANCEL_TEXT)) _cancelled = true; top.Running = false; })
                    }
                    : new MenuItem []
                    {
                        new MenuItem("_Close", "", () =>{ if (Quit("Close", CLOSE_TEXT)) top.Running = false; })
                    })
            });
            top.Add(menu);

            var gridHeaders = applicationData.DataTable.DataColumns.Select((c) => c.Label).ToList();
            win.Add(new Label(GetPaddedString(gridHeaders, top.Frame.Width - 3)));
            

            var items = new List<string>();
            foreach (DataTableRow dataTableRow in applicationData.DataTable.Data)
            {
                var valueList = new List<string>();
                foreach (var dataTableColumn in applicationData.DataTable.DataColumns)
                {
                    valueList.Add(dataTableRow.Values[dataTableColumn.ToString()].DisplayValue);
                }

                // If we have PassThru, then we want to make them selectable. If we make them selectable,
                // they have a 7 character addition of a checkbox ("    [ ]") that we have to factor in.
                int offset = applicationData.PassThru ? 7 : 3;
                items.Add(GetPaddedString(valueList, top.Frame.Width - 3, offset));
            }
            var list = new ListView(items)
            {
                X = 3,
                Y = 3,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2),
                AllowsMarking = applicationData.PassThru
            };
            
            win.Add(list);

            Application.Run();

            if (_cancelled)
            {
                return;
            }

            for (int i = 0; i < applicationData.DataTable.Data.Count; i++)
            {
                if(list.Source.IsMarked(i))
                {
                    SelectedIndexes.Add(i);
                }
            }
        }

        public void Close()
        {
            // top.Running = false;
        }

        private static bool Quit(string title, string text)
        {
            var n = MessageBox.Query(50, 7, title, text, "Yes", "No");
            return n == 0;
        }

        private static string GetPaddedString(List<string> strings, int maxWidth, int offset = 0)
        {
            int colWidth = maxWidth / strings.Count;
            var builder = new StringBuilder();
            foreach (var str in strings)
            {
                for (int i = 0; i < colWidth - str.Length; i++)
                {
                    builder.Append(' ');
                }
                builder.Append(str);
            }

            if (offset > 0)
            {
                builder.Remove(0, offset);
            }

            return builder.ToString();
        }
    }
}
