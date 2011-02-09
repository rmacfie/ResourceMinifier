properties {
    $projectName = "ResourceMinifier"
    $base_dir = resolve-path .
    $sln_file = "$base_dir\src\$projectName.sln"
    $build_dir = "$base_dir\build"
    $debug_dir = "$build_dir\debug"
    $release_dir = "$build_dir\release";
}

$framework = '4.0'

task default -depends debug,release

task clean {
    remove-item -force -recurse $debug_dir -ErrorAction SilentlyContinue
    remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue
 }
task init -depends clean {
    new-item $debug_dir -itemType directory
    new-item $release_dir -itemType directory
}
task debug -depends init {
    msbuild $sln_file "/nologo" "/t:Rebuild" "/p:Configuration=Debug" "/p:OutDir=""$debug_dir\\"""
}
task release -depends init {
    msbuild $sln_file "/nologo" "/t:Rebuild" "/p:Configuration=Release" "/p:OutDir=""$release_dir\\"""
}
