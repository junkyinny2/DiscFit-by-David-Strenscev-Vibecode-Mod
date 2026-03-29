# DiscFit - Feature Log

DiscFit is a lightweight, modern Windows desktop application designed to solve the "Bin Packing" problem for physical storage media (CDs, DVDs, Blu-rays). It calculates how to optimally distribute a massive list of files across multiple discs to minimize wasted space.

## Core Features
*   **Intelligent Bin Packing:** Utilizes the "Best Fit Descending" algorithm to mathematically calculate the most efficient way to group files onto discs.
*   **Lightning Fast Performance:** The core engine executes precisely in $O(N)$ linear time, allowing you to feed it massive directories totaling tens of thousands of files and instantly compute the perfect packing configuration without hanging or memory-shifting wait times. Extension-based icon caching reduces thousands of shell calls to ~20-30 total, keeping icon loading virtually instant.
*   **Native Dark Mode:** Fully supports an immersive Windows 11 Dark Mode toggle with near-instant switching even with hundreds of result sets open. Custom-engineered rendering handlers ensure that even notoriously stubborn legacy UI elements (like tabs, dropdown buttons, and scrollbars) display seamlessly without bright white borders or artifacts. The toggle label provides a consistent "Dark Mode" text regardless of activation state.
*   **Media Size Selection:** Choose from standard pre-defined optical media sizes (CD-R 700MB, DVD±R 4.7GB, DVD±R DL 8.5GB, BD-R 25GB, BD-R DL 50GB) or type in a custom exact byte capacity.
*   **Async Drag-and-Drop:** Drag and drop individual files or entire large folders (11GB+) directly into the application. File scanning runs on a background thread with real-time status bar progress updates (e.g., "Scanning... 4,500 files found"), keeping the UI fully responsive throughout.
*   **64-Bit Standalone Portable Executable:** Upgraded to a 64-bit architecture to eliminate 32-bit memory limits when processing massive datasets. Distributed as a clean, single-file `.exe` with embedded debug symbols, requiring no messy dependency folders.
*   **Modern Interface:** A clean, flat, Windows-native interface utilizing Segoe UI typography.
*   **Detailed Analytics:** 
    *   Hover over any generated disc set tab (e.g., "Set 1") to instantly see its exact capacity in bytes via a ToolTip.
    *   A dynamic status bar calculates and displays the total size of the currently active tab.
    *   Items that physically cannot fit on the selected media type (e.g., a 10GB file trying to fit on a 4.7GB DVD) are automatically separated into an "Oversized" tab.
*   **Live Status Bar Progress:** Real-time feedback during all major operations — file scanning, bin packing, and set building — with a final completion summary (e.g., "Packing complete — 3 sets").
*   **Editable Result Sets:** After packing, modify the last result set on-the-fly by adding or removing files without re-running the entire packing algorithm.
*   **Crash Logging:** Any unhandled errors are caught by a global exception handler, displayed to the user, and logged with full stack traces to `%TEMP%\DiscFit_crash.log` for easy diagnostics.

## Export & Burning Integrations
By right-clicking anywhere inside a generated Disc Set, you can access powerful export options:
*   **Export to CDBurnerXP (.dxp):** Instantly generates an XML compilation file for the selected disc set. When opened in CDBurnerXP, it automatically loads your files and **perfectly preserves the original nested directory structure** of your files from your hard drive.
*   **Copy Set to Folder:** Physically copies all the files from a specific disc set into a new destination folder on your hard drive, automatically rebuilding the nested directory structure. This acts as a perfect workaround for burning software like Nero Burning ROM that does not support dragging-and-dropping file lists while retaining folder structures. *Now fully integrated with the native Windows copy dialog API to show visual progress estimations.*
*   **Export to Text (.txt):** Generates a simple, plain-text file listing the absolute file paths for every file slated to be burned to that specific disc (one path per line).
*   **Export to ImgBurn (.ibb):** Generates an ImgBurn Build project file for the selected disc set, preserving the original nested directory structure. When opened in ImgBurn's Build mode, it automatically loads your files for direct burning.
*   **Save Set as ISO (.iso):** Directly creates an ISO 9660 image (with Joliet long filename support) of an entire disc set in one click. Implemented using the native Windows IMAPI2 API — zero external dependencies required.
*   **Native Drag-and-Drop:** Select files within a generated set and drag them directly out of the application into Windows Explorer or other compatible software.