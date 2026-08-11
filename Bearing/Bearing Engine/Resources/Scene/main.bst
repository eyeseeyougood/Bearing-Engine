(GameObject){
    name: "Scene Root"
    immediateChildren: [
        (GameObject){
            name: "___UI Manager___",
            components: [
                (EditorUI){}
            ],
            metadata: ["___HIDEFROMHIERARCHY___"]
        },

        (GameObject){
            name: "___Camera___",
            components: [
                (CameraMovement){
                    normalSpeed: 3.0,
                    fastSpeed: 7.0,
                    mouseSenseX: 0.2,
                    mouseSenseY: 0.2
                }
            ],
            metadata: ["___HIDEFROMHIERARCHY___"]
        },

        (GameObject){
            name: "Cube",
            transform: (Transform3D){position:(Vector3:0,0,0){},scale:(Vector3:1.5,1.5,1.5){}},
            components: [
                (MeshRenderer:"Cube.obj"){
                    material: (Material){
                        shader: (Shader:"eng/default.vert","eng/default.frag"){},
                        parameters: [(ShaderParam){name: "mainColour", value: [0.0, 0.3, 1.0, 1.0]}]
                    }
                }
            ],
            immediateChildren: [
                (GameObject){
                    name: "Child Cube",
                    transform: (Transform3D){position:(Vector3:1.5,0,0){},scale:(Vector3:1,1.5,1){}},
                    components: [
                        (MeshRenderer:"Cube.obj"){
                            material: (Material){
                                shader: (Shader:"eng/default.vert","eng/default.frag"){},
                                parameters: [(ShaderParam){name: "mainColour", value: [1.0, 0.3, 0.0, 1.0]}]
                            }
                        },
                        (TestComponent){},
                        (TestComponent){}
                    ]
                }
            ]
        },

        (GameObject){
            name: "Cube 2",
            components: [
                (MeshRenderer:"Cube.obj"){
                    material: (Material){
                        shader: (Shader:"eng/default.vert","eng/default.frag"){},
                        parameters: [(ShaderParam){name: "mainColour", value: [0.6, 0.3, 1.0, 1.0]}]
                    }
                }
            ]
        }
    ]
}