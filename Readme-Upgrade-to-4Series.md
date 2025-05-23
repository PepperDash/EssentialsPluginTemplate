![PepperDash Essentials Plugin Logo](/images/essentials-plugin-blue.png)

# Upgrading an EPI: Essentials v1.x → v2.x

## Preparation

1. **Clone the Template Repo referred to as TR** to your local machine.
    - [EssentialsPluginTemplate](https://github.com/PepperDash/EssentialsPluginTemplate) 
2. **Checkout `main` branch** in the TR.

## Branching

4. In your EPI repo:
   - Confirm it does **not already support Essentials v2.x**.
   - Ensure there is **no existing 4-series update branch**.
   - Create a new branch (e.g., `feature/4-series-updates`) from `main`.

## Upgrade Steps

5. In the new branch:
   - **Replace** EPI `.github` files/folders with those from TR as needed.
     - *Note:* Retain any custom workflows if required. Consult with PepperDash Team if unsure.
   - **Copy** the following from TR to EPI:
     - `.releaserc.json` (root)
     - `epi-make-model.4Series.sln` (root)
     - `src/epi-make-model.4Series.csproj` (create `src` folder if missing)
     - `images` folder (if your `readme.md` lacks the standard logo)
   - **Initial Commit:** Commit changes, but do **not** push yet.

6. **If EPI has a `src/*.nuspec` file:**
   - Record these values from `Nuspec` for later:
     - `<id>` (Package ID)
     - `<projectUrl>` (Project URL)
     - `<title>` (Assembly Title)

7. **Open Solution:**
   - Open `epi-make-model.4Series.sln` in Visual Studio 2022.
   - If errors occur, ensure the `.csproj` file is located within `src` folder.

8. **Update Project Properties:**
   - In `src/epi-make-model.4Series.csproj`, update:
     - `AssemblyTitle` (from nuspec `<title>`)
     - `RootNamespace` (from existing EPI, device namespace actual)
     - `PackageProjectUrl` (from nuspec `<projectUrl>`)
   - **Rename `make-model` build path references shown below:**  

     ![defineBuildObjectstoRemove](/images/upgrade-to-4Series/removeObsoleteReferences.png)
   - **Add runtime exclusion:**  
     Add `<ExcludeAssets>runtime</ExcludeAssets>` to the PepperDash Essentials `PackageReference`. 

     ![addRuntimeExclusion](/images/upgrade-to-4Series/addRuntumeExclusion.png)

9. **Copy Build Files:**
   - Copy `src/Directory.Build.props` and `src/Directory.Build.targets` from TR.
   - Update `RepositoryUrl` in `Directory.Build.props` to match nuspec `PackageProjectUrl`.
   - Remove "template" text from `<Product>` in `Directory.Build.props` as needed.

10. **Clean Up:**
    - Delete old Visual Studio 2008 files: `.sln`, `.suo`, `.nuspec`, `.csproj`, `.csproj.user`, `.projectinfo`.

11. **Rename Solution/Project** as needed.  

    ![renameEpiSolution](/images/upgrade-to-4Series/renameEpiSolution.png)

12. **Update Project References and Dependencies**
    - Remove old PepperDash Essentials dependency.
    - Add PepperDash Essentials v2.x dependency via Visual Studio 2022 > Project > `Manage Nuget Packages`.  

    ![removeExistingReferences](/images/upgrade-to-4Series/removeExistingDependencies.png)

13. **Update Factory Class:**  
    Set `MinimumEssentialsFrameworkVersion` to match the Essentials version dependency.

14. **Build Solution:**  
    Ensure solution compiles (warnings about obsolete `Debug.Console` calls are OK).

15. **Update Debug Calls:**  
    Replace `Debug.Console` with `Debug.Log` methods as appropriate (see table). 

    ![replaceDebugConsoleMethods](/images/upgrade-to-4Series/replaceExistingDebugConsoleMethods.png)

## Completion Checklist

- [ ] EPI compiles with **no warnings**.
- [ ] **Obsolete methods** updated to modern equivalents.
- [ ] **Workflows** complete successfully; packages generated via GitHub and NuGet.org.
- [ ] `readme.md` includes:
  - Usage/setup instructions
  - Example SIMPL Windows bridge joinmap
  - Example device communication structure/properties
- [ ] EPI tested by another developer.

---

*For questions about workflows or customizations, consult with PepperDash Team.*