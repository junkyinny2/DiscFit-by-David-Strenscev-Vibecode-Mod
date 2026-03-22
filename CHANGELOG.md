# DiscFit - Changelog

## Version 1.7
*   **Enhancement: Native Windows Copy Dialog:** Overhauled the 'Copy Set to Folder' functionality to utilize the `SHFileOperation` Windows API instead of a silent background array of single-file copies. Copying large media sets will now intuitively trigger the familiar, native Windows copy progress dialog, replete with time estimations and identical overwrite prompting natively expected by users.
*   **UI Enhancements: Dark Mode Text:** Fixed the Dark Mode visual theme toggle checkbox so its text remains a permanent, non-dynamic "Dark Mode" regardless of whether the theme is checked or unchecked.

## Version 1.6
*   **Optimization: High-Speed Packing Engine:** Completely rewrote the core 'Best-Fit' bin packing algorithm, replacing an incredibly slow $O(N^2)$ array-shifting bottleneck with a modern $O(N)$ linear iteration. The engine can now compute configurations for over 50,000 files virtually instantaneously.
*   **Bug Fix: Integer Underflow:** Patched a severe `ulong` unsigned integer arithmetic vulnerability inside the capacity calculation that triggered impossible negative capacities when processing massively oversized files.
*   **Feature: Dark Mode Theme:** Implemented a system-native immersive Windows 11 Dark Mode theme toggle.
*   **UI Enhancements: Stubborn Artifacts:** Engineered custom `DarkTabControl` and `DarkComboBox` rendering subclasses to manually intercept OS paint commands, effectively hiding and overwriting stubborn, unsupported Win32 visual style artifacts (like white 3D borders and light-dropdown boxes).
*   **UI Enhancements: Centered Title Bar:** Integrated custom text alignment padding utilizing Non-Breaking Spaces (`\u00A0`) alongside scaling logic to perfectly center the application title natively, defying native Windows 11 DWM whitespace-trimming security behaviors.

## Version 1.5
*   **Rebranding:** Globally renamed the project, source files, and namespaces from "BinPacking" to "DiscFit". 
*   **Title Bar Update:** Updated the application title bar to display "DiscFit - Version 1.5 - David Strencsev".
*   **UI Overhaul:** Implemented a modern, flat UI design. Removed outdated 3D borders, updated the color palette to a clean light-gray/white theme, styled the "Pack" button with a flat blue accent, and applied `Segoe UI` typography globally to seamlessly match modern Windows aesthetics.
*   **Dynamic Analytics:** Added a dynamic Status Bar to the bottom right of the window that actively calculates and displays the total byte size of the currently selected disc tab.
*   **Tab ToolTips:** Added ToolTips to the generated disc tabs. Hovering over a tab (e.g., "Set 1") now displays the exact total byte size of the files contained within that disc.

## Version 1.4
*   **Feature: Copy Set to Folder:** Added a new context menu option that physically copies all files from a generated disc set into a user-selected destination directory. This perfectly reconstructs the original nested folder structure, serving as a native workaround for Nero Burning ROM's inability to retain folder structures via drag-and-drop.
*   **Feature: Export to Text:** Added a new context menu option to generate a `.txt` file containing a raw list of absolute file paths for a selected disc set.

## Version 1.3
*   **Enhancement: CDBurnerXP Directory Structures:** Completely rewrote the CDBurnerXP `.dxp` XML generator. The exporter now parses the absolute paths of the files and dynamically reconstructs the original nested XML directory tree, ensuring folders are perfectly preserved when the file is opened in CDBurnerXP.

## Version 1.2
*   **Feature: Context Menus:** Added right-click context menus to the dynamically generated ListView controls for Disc Sets and the Oversized list.
*   **Feature: Export to CDBurnerXP:** Added the foundational "Export to CDBurnerXP (.dxp)" functionality.
*   **Bug Fix: Folder Drag-and-Drop:** Fixed a critical flaw where dragging a large folder into the application treated the entire folder as a single, indivisible object. Folders are now correctly recursively scanned, and their individual files are extracted and added to the list for proper mathematical bin packing.
*   **Bug Fix: UI Path Display:** Fixed variable scoping bugs that caused the "Path" column in the ListViews to incorrectly display only the file name or drop the path entirely. 
*   **Bug Fix: Icon Crash:** Resolved an unhandled `ArgumentException` crash that occurred when `SHGetFileInfo` failed to return a valid icon handle for certain files (e.g., missing files or paths that are too long). Implemented a system fallback icon to prevent crashes.