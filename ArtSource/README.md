# Blender source assets

Editable `.blend` sources live here, outside Unity's `Assets` directory.
`Characters/BaseHuman`, `Characters/Electrician`, `Characters/Viggo`, `Props`, and
`Environment` are reserved directories; production has not started.

`Tests/C0/TEST_C0_MetreMesh.blend` is a generated pipeline fixture, not approved art.
Its ground origin, applied transforms and raised forward nose test axis conversion.
Blender Z is up and +Y is forward; Unity Y is up and +Z is forward.
The body is 1 m wide, 2 m tall and 0.5 m deep. Including the nose, Unity bounds
are size `(1, 2, 1)`, centre `(0, 1, 0.25)`.

See [pipeline instructions](../Tools/Blender/README.md). Approved images in
`ArtReferences/` remain canonical and are never modified by this pipeline.
