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
            name: "Light",
            transform: (Transform3D){position:(Vector3:2,2,2){}},
            components: [
                (PointLight){
                    intensity: 0.5
                }
            ],
        },

        (GameObject){
            name: "Cube",
            components: [
                (MeshRenderer:"eng/ICOSphere.obj"){
                    material: (Material){
                        shader: (Shader:"eng/default.vert","eng/lighting.frag"){},
                        parameters: [(ShaderParam){name: "mainColour", value: [0.9, 0.9, 0.9, 1.0]}]
                    }
                }
            ]
        }
    ]
}