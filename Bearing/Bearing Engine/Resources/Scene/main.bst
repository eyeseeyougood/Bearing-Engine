(GameObject){
    name: "Scene Root",
    #defaultTransform.bst#
    immediateChildren: [
        (GameObject){
            name: "Triangle",
            transform: (Transform3D){
                position:  (Vector3: 0, -2, 0) {},
                eRotation: (Vector3: 0,  0, 0) {},
                scale:     (Vector3: 1,  1, 1) {},
            },
            components: [
                (MeshRenderer:"eng/Cube.obj"){
                    material: (Material){
                        shader: (Shader:"eng/default.vert","eng/lighting.frag"){},

                        parameters: [
                            (ShaderParam){
                                name: "mainColour",
                                value: [0.7, 0.1, 0.1, 1.0],
                            }
                        ]
                    },
                },
                (BearingRigidbody){
                    Collider: (BoxShape: 0.5){}
                },
            ]
        },

        (GameObject){
            name: "Light",
            transform: (Transform3D){
                position:  (Vector3: 2, 2, 0) {},
                eRotation: (Vector3: 0, 0, 0) {},
                scale:     (Vector3: 1, 1, 1) {},
            },
            components: [
                (PointLight){
                    range: 10,
                    intensity: 1,
                },
            ]
        },


        (GameObject){
            name: "Heirarchy",
            tag: "EditorObject",
            #defaultTransform2D.bst#,
            components: [
                (UIPanel){
                    renderLayer: -3,
                    anchor: (Vector2:0,0){},

                    position: (UDim2){
                        scale: (Vector2:0,0){}
                    },

                    size: (UDim2){
                        scale: (Vector2:0.2,1.0){}
                    },

                    rid: 6,
                },

                (UIVerticalScrollView){
                    parent: 6,
                    renderLayer: -2,
                    anchor: (Vector2:0,0){},

                    position: (UDim2){
                        scale: (Vector2:0,0){}
                    },

                    size: (UDim2){
                        scale: (Vector2:1,1){},
                        offset: (Vector2:0,-100){}
                    },

                    rid: 1,
                    clipContents: false,
                    scrollSensitivity: 15,
                    scrollByComponents: false,
                },

                (UIButton){
                    parent: 6,
                    renderLayer: 100,
                    anchor: (Vector2:0,1){},

                    position: (UDim2){
                        scale: (Vector2:0,1){}
                    },

                    size: (UDim2){
                        scale: (Vector2:1,0){},
                        offset: (Vector2:0,100){}
                    },

                    rid: 7,
                },

                (UILabel){
                    parent: 6,
                    renderLayer: 101,
                    anchor: (Vector2:0,1){},

                    position: (UDim2){
                        scale: (Vector2:0,1){}
                    },

                    size: (UDim2){
                        scale: (Vector2:1,0){},
                        offset: (Vector2:0,100){}
                    },

                    rid: 8,
                    font: "Times New Roman",
                    text: "Add GameObject",
                },

                (Hierarchy){}
            ]
        },


        (GameObject){
            name: "Inspector",
            tag: "EditorObject",
            #defaultTransform2D.bst#,
            components: [
                (UIPanel){
                    renderLayer: -3,
                    anchor: (Vector2:1,0){},

                    position: (UDim2){
                        scale: (Vector2:1,0){}
                    },

                    size: (UDim2){
                        scale: (Vector2:0.2,1.0){}
                    },

                    rid: 3,
                },

                (UIVerticalScrollView){
                    parent: 3,
                    renderLayer: -2,
                    anchor: (Vector2:0,0){},

                    position: (UDim2){
                        scale: (Vector2:0,0){}
                    },

                    size: (UDim2){
                        scale (Vector2:1,1){},
                        offset: (Vector2:0,-100){}
                    },

                    rid: 2,
                    clipContents: false,
                    scrollSensitivity: 7,
                    scrollByComponents: false,
                },

                (UIButton){
                    parent: 3,
                    renderLayer: 100,
                    anchor: (Vector2:0,1){},

                    position: (UDim2){
                        scale: (Vector2:0,1){}
                    },

                    size: (UDim2){
                        scale: (Vector2:1,0){},
                        offset: (Vector2:0,100){}
                    },

                    rid: 4,
                },

                (UILabel){
                    parent: 3,
                    renderLayer: 101,
                    anchor: (Vector2:0,1){},

                    position: (UDim2){
                        scale: (Vector2:0,1){}
                    },

                    size: (UDim2){
                        scale: (Vector2:1,0){},
                        offset: (Vector2:0,100){}
                    },

                    rid: 5,
                    font: "Times New Roman",
                    text: "Add Component",
                },

                (Inspector){},

                (SceneExporter){},

                (PresetImporter){},

                (CameraMovement){}
            ]
        }
    ]
}