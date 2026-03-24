Original Discfit 1.1 by David Strenscev - Open source.

Vibecoded with Googles Antigravity to add features and run under Windows 11


<img width="1578" height="476" alt="image" src="https://github.com/user-attachments/assets/6e90dbc9-8f5c-4bcb-b46b-e892d12da5eb" />


# DiscFit - Feature Log

DiscFit is a lightweight, modern Windows desktop application designed to solve the "Bin Packing" problem for physical storage media (CDs, DVDs, Blu-rays). It calculates how to optimally distribute a massive list of files across multiple discs to minimize wasted space.

## Core Features
*   **Intelligent Bin Packing:** Utilizes the "Best Fit Descending" algorithm to mathematically calculate the most efficient way to group files onto discs.
*   **Lightning Fast Performance:** The core engine executes precisely in $O(N)$ linear time, allowing you to feed it massive directories totaling tens of thousands of files and instantly compute the perfect packing configuration without hanging or memory-shifting wait times.
*   **Native Dark Mode:** Fully supports an immersive Windows 11 Dark Mode toggle. Custom-engineered rendering handlers ensure that even notoriously stubborn legacy UI elements (like tabs and dropdown buttons) display seamlessly without bright white borders or artifacts. The toggle label provides a consistent "Dark Mode" text regardless of activation state.
*   **Media Size Selection:** Choose from standard pre-defined optical media sizes (CD-R 700MB, DVD±R 4.7GB, DVD±R DL 8.5GB, BD-R 25GB, BD-R DL 50GB) or type in a custom exact byte capacity.
*   **Drag-and-Drop Support:** Drag and drop individual files or entire large folders directly into the application. Folders are automatically and recursively scanned to extract all individual files.
*   **Standalone Portable Executable:** Distributed as a clean, single-file `.exe` with embedded debug symbols, requiring no messy dependency folders.
*   **Modern Interface:** A clean, flat, Windows-native interface utilizing Segoe UI typography.
*   **Detailed Analytics:** 
    *   Hover over any generated disc set tab (e.g., "Set 1") to instantly see its exact capacity in bytes via a ToolTip.
    *   A dynamic status bar calculates and displays the total size of the currently active tab.
    *   Items that physically cannot fit on the selected media type (e.g., a 10GB file trying to fit on a 4.7GB DVD) are automatically separated into an "Oversized" tab.

## Export & Burning Integrations
By right-clicking anywhere inside a generated Disc Set, you can access powerful export options:
*   **Export to CDBurnerXP (.dxp):** Instantly generates an XML compilation file for the selected disc set. When opened in CDBurnerXP, it automatically loads your files and **perfectly preserves the original nested directory structure** of your files from your hard drive.
*   **Copy Set to Folder:** Physically copies all the files from a specific disc set into a new destination folder on your hard drive, automatically rebuilding the nested directory structure. This acts as a perfect workaround for burning software like Nero Burning ROM that does not support dragging-and-dropping file lists while retaining folder structures. *Now fully integrated with the native Windows copy dialog API to show visual progress estimations.*
*   **Export to Text (.txt):** Generates a simple, plain-text file listing the absolute file paths for every file slated to be burned to that specific disc (one path per line).
*   **Native Drag-and-Drop:** Select files within a generated set and drag them directly out of the application into Windows Explorer or other compatible software.

# DiscFit - Changelog

## Version 1.7
*   **Build Optimization: Single-File Executable:** Configured the `.csproj` to support framework-dependent single-file publishing and embedded the `.pdb` debug symbols directly into the executable, ensuring clean, standalone `.exe` distributions.
*   **Codebase Enhancement: Localization:** Translated all auto-generated Visual Studio boilerplate comments (e.g., in `Program.cs`, `AssemblyInfo.cs`) from Spanish to English, ensuring the entire source code is fully localized in English.
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


Summary

**Rebranding & UI**
* The project was renamed from "BinPacking" to "DiscFit" (v1.5).
* A modern, flat UI design was implemented (v1.5), replacing outdated 3D borders, and a system-native Windows 11 Dark Mode theme was added (v1.6).
* The application title was centered natively (v1.6), and a dynamic Status Bar was added to calculate and display the total byte size of the selected disc tab (v1.5).
* ToolTips were added to generated disc tabs to show exact byte sizes (v1.5).

**Core Algorithm & Performance**
* The core 'Best-Fit' bin packing algorithm was completely rewritten from a slow $O(N^2)$ process to a high-speed $O(N)$ linear iteration, allowing it to process over 50,000 files virtually instantaneously (v1.6).
* Fixed integer underflow bugs that crashed the app during massive file operations (v1.6) and resolved icon crashes for missing files or long paths (v1.2).

**Exporting & File Operations**
* Added the ability to export disc sets to CDBurnerXP (`.dxp`) (v1.2), and rewrote the exporter to perfectly reconstruct original XML directory structures (v1.3).
* Added "Copy Set to Folder" to recreate folder structures in a destination directory (v1.4) which was later upgraded to use the native Windows File Operation UI with progress Dialogs (v1.7).
* Added the ability to export lists of files to raw `.txt` files (v1.4).
* Fixed folder drag-and-drop to properly unpack files inside folders recursively (v1.2).

**Compilation**
* The application is now published as a framework-dependent single-file executable with embedded debug symbols for easier distribution (v1.7).
*   **Bug Fix: UI Path Display:** Fixed variable scoping bugs that caused the "Path" column in the ListViews to incorrectly display only the file name or drop the path entirely. 
*   **Bug Fix: Icon Crash:** Resolved an unhandled `ArgumentException` crash that occurred when `SHGetFileInfo` failed to return a valid icon handle for certain files (e.g., missing files or paths that are too long). Implemented a system fallback icon to prevent crashes.

