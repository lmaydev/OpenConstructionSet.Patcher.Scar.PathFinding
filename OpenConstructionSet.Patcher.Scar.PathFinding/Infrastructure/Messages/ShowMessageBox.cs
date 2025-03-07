﻿using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages
{
    internal record struct ShowMessageBox(string Message, string Title, MessageBoxImage Image, MessageBoxButton Button = MessageBoxButton.OK);
}