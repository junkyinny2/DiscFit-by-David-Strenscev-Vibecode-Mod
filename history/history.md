# Project History & Milestones 📜

> [!IMPORTANT]
> **🤖 AI HANDOFF NOTES & CURRENT STATE**
> *AI INSTRUCTION: Establish the "Source of Truth" for all subsequent models.*

**Project Persona**: Lightweight Windows Bin Packing application for media backup.
**Tech Stack**: .NET 8.0 Windows, WinForms (C#), x64 Architecture.
**Design Language**: Custom Modern Dark Theme (DWM Immersive Dark Mode) with centered title bar.
**Repository**: [Not provided]

### ✅ Verification Protocol
- **Build**: Use `build.bat` to publish standalone win-x64 executable.
- **Visuals**: Title bar should display version (currently v1.13).

### 🚧 What's In Progress
- [x] **Active Task**: Support for empty folders and removal of InfraRecorder export (v1.11).
- [x] **Active Task**: ISO export via native IMAPI2 (v1.12) — complete.

---

### 🕒 Change Log (Git-Style)
- **feat(export)**: [2026-03-28] v1.12: Added native ISO Export using Windows IMAPI2 (Zero-Dependency).
- **feat(core)**: [2026-03-28] v1.11: Added full support for empty folders in drag-drop and folder scanning.
- **fix(copy)**: [2026-03-28] v1.11: Fixed crash when copying sets containing directories by using shell operation logic correctly.
- **chore(cleanup)**: [2026-03-28] v1.11: Removed InfraRecorder export functionality as requested.
- **feat(export)**: [2026-03-28] Updated DXP and ImgBurn exporters to support directory nodes.
- **feat(export)**: [2026-03-28] v1.10: Added ImgBurn (.ibb) and InfraRecorder (.irp) export (InfraRecorder later removed in v1.11).
- **feat(arch)**: [2026-03-27] Migrated to x64 and enabled Single-File Publishing.

---

### 📅 Future Roadmap
- [ ] **v1.13**: Potential UI performance refinements for massive datasets (>100k files).
- [ ] **Optimization**: Multithreaded packing calculation for faster results on huge file sets.
