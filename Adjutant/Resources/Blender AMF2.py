bl_info = {
    "name": "AMF format",
    "description": "Import AMF files created by Reclaimer.",
    "author": "Gravemind2401",
    "version": (2, 0, 4),
    "blender": (2, 80, 0),
    "location": "File > Import > AMF",
    "category": "Import-Export",
}

import struct
import io
import bpy
import bmesh
import math
import os.path as ospath
import itertools
import bpy_extras

from pathlib import Path, PureWindowsPath
from dataclasses import dataclass
from mathutils import Matrix, Quaternion, Vector
from bpy.props import BoolProperty, FloatProperty, StringProperty, EnumProperty

@dataclass
class FileReader:
    __stream: io.BufferedReader

    def __init__(self, file_name):
        print(f"open file: {file_name}")
        self.__stream = open(file_name, "rb") # "rb" - open for reading in binary mode

    def tell(self):
        return self.__stream.tell()

    def seek(self, dest, start = 0):
        self.__stream.seek(dest, start)

    def close(self):
        print(f"closed file")
        self.__stream.close()

    def read_byte(self, endian = "<"):
        return struct.unpack(endian + "B", self.__stream.read(1))[0]

    def read_ushort(self, endian = "<"):
        return struct.unpack(endian + "H", self.__stream.read(2))[0]

    def read_short(self, endian = "<"):
        return struct.unpack(endian + "h", self.__stream.read(2))[0]

    def read_uint(self, endian = "<"):
        return struct.unpack(endian + "I", self.__stream.read(4))[0]

    def read_int(self, endian = "<"):
        return struct.unpack(endian + "i", self.__stream.read(4))[0]

    def read_float(self, endian = "<"):
        return struct.unpack(endian + "f", self.__stream.read(4))[0]

    def read_vec2(self, endian = "<"):
        return Vector([self.read_float(endian), self.read_float(endian)])

    def read_vec3(self, endian = "<"):
        return Vector([self.read_float(endian), self.read_float(endian), self.read_float(endian)])

    def read_vec4(self, endian = "<"):
        return Vector([self.read_float(endian), self.read_float(endian), self.read_float(endian), self.read_float(endian)])

    def read_quat(self, endian = "<"):
        vec = self.read_vec4(endian)
        return Quaternion([vec.w, vec.x, vec.y, vec.z])

    def read_string(self):
        chars = []
        next = self.__stream.read(1).decode()
        while next != chr(0):
            chars.append(next)
            next = self.__stream.read(1).decode()
        return "".join(chars)

@dataclass
class Vertex:
    position: Vector
    normal: Vector
    texcoords: Vector
    indices: Vector
    weights: Vector

    def __init__(self, reader, vertex_format):
        self.position = reader.read_vec3()
        self.normal = reader.read_vec3()
        self.texcoords = reader.read_vec2()
        
        #these normals arent normal enough for Blender
        self.normal.normalize()

        #the vertex may be affected by 0 to 4 bones
        #the file stores the index of each relevant bone,
        #terminated by a 0xFF if not all bones were used

        # 0 = no skinning
        # 1 = weighted skinning
        # 2 = rigid skinning
        if vertex_format == 0:
            self.indices = None
            self.weights = None
        else:
            temp = [reader.read_byte(), reader.read_byte()]
            if temp[1] != 255:
                temp.append(reader.read_byte())
                if temp[2] != 255:
                    temp.append(reader.read_byte())
            count = len(temp)
            if temp[count - 1] == 255: # dont count trailing terminators
                count -= 1
            if count == 1:
                self.indices = Vector([float(temp[0]), 0])
                if vertex_format == 1:
                    self.weights = Vector([reader.read_float(), 0])
                else:
                    self.weights = Vector([1, 0])
            elif count == 2:
                self.indices = Vector([float(temp[0]), float(temp[1])])
                if vertex_format == 1:
                    self.weights = reader.read_vec2()
                else:
                    self.weights = Vector([1, 1])
            elif count == 3:
                self.indices = Vector([float(temp[0]), float(temp[1]), float(temp[2])])
                if vertex_format == 1:
                    self.weights = reader.read_vec3()
                else:
                    self.weights = Vector([1, 1, 1])
            else: #count == 4:
                self.indices = Vector([float(temp[0]), float(temp[1]), float(temp[2]), float(temp[3])])
                if vertex_format == 1:
                    self.weights = reader.read_vec4()
                else:
                    self.weights = Vector([1, 1, 1, 1])

@dataclass
class AmfModel:
    header: int
    version: float
    name: str
    nodes: []
    markers: []
    regions: []
    materials: []
    vertex_buffers: dict
    index_buffers: dict

    def __init__(self, reader):
        self.__read_header(reader)
        self.__read_nodes(reader)
        self.__read_markers(reader)
        self.__read_regions(reader)
        self.__read_materials(reader)

        self.vertex_buffers = dict()
        self.index_buffers = dict()

        print("reading mesh data")
        for region in self.regions:
            for perm in region.permutations:
                if perm.vert_address not in self.vertex_buffers:
                    self.vertex_buffers[perm.vert_address] = self.__read_vertices(reader, perm)
                if perm.face_address not in self.index_buffers:
                    self.index_buffers[perm.face_address] = self.__read_indices(reader, perm)

    def __read_header(self, reader):
        print("reading header")
        self.header = reader.read_int()
        self.version = reader.read_float()
        self.name = reader.read_string()

        if self.version < 2.0:
            raise Exception("AMF files must be v2.0 or newer.")

    def __read_nodes(self, reader):
        print("reading nodes")
        self.nodes = []
        nodes_count = reader.read_int()
        nodes_address = reader.read_int()
        pos = reader.tell()
        reader.seek(nodes_address)
        for i in range(nodes_count):
            self.nodes.append(Node(reader))
        reader.seek(pos)

    def __read_markers(self, reader):
        print("reading markers")
        self.markers = []
        groups_count = reader.read_int()
        groups_address = reader.read_int()
        pos = reader.tell()
        reader.seek(groups_address)
        for i in range(groups_count):
            self.markers.append(MarkerGroup(reader))
        reader.seek(pos)

    def __read_regions(self, reader):
        print("reading regions")
        self.regions = []
        regions_count = reader.read_int()
        regions_address = reader.read_int()
        pos = reader.tell()
        reader.seek(regions_address)
        for i in range(regions_count):
            self.regions.append(Region(reader))
        reader.seek(pos)

    def __read_materials(self, reader):
        print("reading materials")
        self.materials = []
        mat_count = reader.read_int()
        mat_address = reader.read_int()
        pos = reader.tell()
        reader.seek(mat_address)
        for i in range(mat_count):
            self.materials.append(Material(reader))
        reader.seek(pos)

    def __read_vertices(self, reader, permutation):
        vertices = []
        pos = reader.tell()
        reader.seek(permutation.vert_address)
        for i in range(permutation.vert_count):
            vertices.append(Vertex(reader, permutation.vert_format))
        reader.seek(pos)
        return vertices

    def __read_indices(self, reader, permutation):
        indices = []
        pos = reader.tell()
        reader.seek(permutation.face_address)
        for i in range(permutation.face_count):
            if permutation.vert_count > 65535:
                indices.append([reader.read_int(), reader.read_int(), reader.read_int()])
            else:
                indices.append([reader.read_ushort(), reader.read_ushort(), reader.read_ushort()])
        reader.seek(pos)
        return indices

@dataclass
class Node:
    name: str
    parent_index: int
    child_index: int
    sibling_index: int
    position: Vector
    rotation: Quaternion

    def __init__(self, reader):
        self.name = reader.read_string()
        self.parent_index = reader.read_short()
        self.child_index = reader.read_short()
        self.sibling_index = reader.read_short()
        self.position = reader.read_vec3()
        self.rotation = reader.read_quat()

@dataclass
class MarkerGroup:
    name: str
    markers: []

    def __init__(self, reader):
        self.name = reader.read_string()
        child_count = reader.read_int()
        child_address = reader.read_int()
        pos = reader.tell()
        reader.seek(child_address)
        self.markers = []
        for i in range(child_count):
            self.markers.append(Marker(reader))
        reader.seek(pos)

@dataclass
class Marker:
    region_index: int
    permutation_index: int
    node_index: int
    position: Vector
    rotation: Quaternion

    def __init__(self, reader):
        self.region_index = reader.read_byte()
        self.permutation_index = reader.read_byte()
        self.node_index = reader.read_short()
        self.position = reader.read_vec3()
        self.rotation = reader.read_quat()

@dataclass
class Region:
    name: str
    permutations: []

    def __init__(self, reader):
        self.name = reader.read_string()
        child_count = reader.read_int()
        child_address = reader.read_int()
        pos = reader.tell()
        reader.seek(child_address)
        self.permutations = []
        for i in range(child_count):
            self.permutations.append(Permutation(reader))
        reader.seek(pos)

@dataclass
class Permutation:
    name: str
    vert_format: int
    node_index: int
    vert_count: int
    vert_address: int
    face_count: int
    face_address: int
    submeshes: []
    scale: float
    transform: Matrix

    def __init__(self, reader):
        self.name = reader.read_string()
        self.vert_format = reader.read_byte()
        self.node_index = reader.read_byte()
        self.vert_count = reader.read_int()
        self.vert_address = reader.read_int()
        self.face_count = reader.read_int()
        self.face_address = reader.read_int()
        child_count = reader.read_int()
        child_address = reader.read_int()
        self.scale = reader.read_float()

        if not math.isnan(self.scale):
            row1 = [reader.read_float(), reader.read_float(), reader.read_float(), 0]
            row2 = [reader.read_float(), reader.read_float(), reader.read_float(), 0]
            row3 = [reader.read_float(), reader.read_float(), reader.read_float(), 0]
            row4 = [reader.read_float(), reader.read_float(), reader.read_float(), 1]
            self.transform = Matrix([row1, row2, row3, row4]).transposed()
        else: self.transform = Matrix()

        pos = reader.tell()
        reader.seek(child_address)
        self.submeshes = []
        for i in range(child_count):
            self.submeshes.append(Submesh(reader))
        reader.seek(pos)


@dataclass
class Submesh:
    mat_index: int
    face_start: int
    face_count: int

    def __init__(self, reader):
        self.mat_index = reader.read_short()
        self.face_start = reader.read_int()
        self.face_count = reader.read_int()

#@dataclass
#class TextureType:
#    diffuse: str = "diffuse"
#    detail: str = "detail"
#    colour_change: str = "colour_change"
#    bump: str = "bump"
#    detail_bump: str = "detail_bump"
#    illum: str = "illum"
#    specular: str = "specular"
#    reflection: str = "reflection"
#
#    @staticmethod
#    def enumerate():
#        return [TextureType.diffuse, TextureType.detail, TextureType.colour_change, TextureType.bump, TextureType.detail_bump, TextureType.illum, TextureType.specular, TextureType.reflection]

@dataclass
class Material:
    name: str
    is_terrain: bool
    is_transparent: bool
    cc_only: bool
    textures: []
    tints: []

    def __init__(self, reader):
        self.name = reader.read_string()
        self.is_terrain = self.name[0] == '*'

        self.is_transparent = False
        self.cc_only = False
        self.textures = []
        self.tints = []

        if not self.is_terrain:
            for i in range(8):
                name = reader.read_string()
                tiles = Vector([0, 0])
                if name != "null":
                    tiles = reader.read_vec2()
                self.textures.append(Submaterial(name, tiles))

            for i in range(4):
                self.tints.append([reader.read_byte(), reader.read_byte(), reader.read_byte(), reader.read_byte()])

            self.is_transparent = reader.read_byte() == 1
            self.cc_only = reader.read_byte() == 1
        else:
            self.name = self.name[1:]
            blend_path = reader.read_string()
            blend_tile = Vector([0, 0])
            if blend_path != "null":
                blend_tile = Vector([reader.read_float(), reader.read_float()])

            base_count = reader.read_byte()
            bump_count = reader.read_byte()
            detail_count = reader.read_byte()

            base_maps = []
            base_tiles = []
            bump_maps = []
            bump_tiles = []
            detail_maps = []
            detail_tiles = []

            for i in range(base_count):
                base_maps.append(reader.read_string())
                base_tiles.append(Vector([reader.read_float(), reader.read_float()]))

            for i in range(bump_count):
                bump_maps.append(reader.read_string())
                bump_tiles.append(Vector([reader.read_float(), reader.read_float()]))

            for i in range(detail_count):
                detail_maps.append(reader.read_string())
                detail_tiles.append(Vector([reader.read_float(), reader.read_float()]))

            self.textures.append(Submaterial(base_maps[0], base_tiles[0]))

@dataclass
class Submaterial:
    texture_path: str
    uv_tiles: Vector
    is_null: bool
    
    def __init__(self, texture_path, uv_tiles):
        self.texture_path = texture_path
        self.uv_tiles = uv_tiles
        self.is_null = texture_path == "null"
    
    def get_full_path(self, root_dir, ext):
        return Path(root_dir) / Path(PureWindowsPath(self.texture_path + "." + ext))
    
    def get_texture_name(self):
        return PureWindowsPath(self.texture_path).name

@dataclass
class ImportOptions:
    IMPORT_SCALE: float = 1.0
    IMPORT_BONES: bool = True
    IMPORT_MARKERS: bool = True
    IMPORT_MESHES: bool = True
    IMPORT_MATERIALS: bool = True

    PREFIX_MARKER: str = "#"
    PREFIX_BONE: str = ""

    DIRECTORY_BITMAP: str = ""
    SUFFIX_BITMAP: str = "tif"

    MODE_SCALE = 'METERS'
    MODE_MARKERS = 'EMPTY'
    MODE_MESHES = 'JOIN'

    def get_scale_multiplier(self):
        if self.MODE_SCALE == 'METERS':
            return self.IMPORT_SCALE * 0.03048
        elif self.MODE_SCALE == 'HALO':
            return self.IMPORT_SCALE * 0.01
        else: # self.MODE_SCALE == 'MAX'
            return self.IMPORT_SCALE

    def get_node_transforms(self, nodes, scale_fix = 1.0):
        transforms = []
        for i, node in enumerate(nodes):
            pos = Matrix.Translation(node.position * self.get_scale_multiplier() * scale_fix)
            rot = node.rotation.to_matrix().to_4x4()
            transform = pos @ rot
            if node.parent_index >= 0:
                transform = transforms[node.parent_index] @ transform
            transforms.append(transform)
        return transforms

    def get_perm_transform(self, perm):
        if not math.isnan(perm.scale):
            #in amf files geometry is stored as 100x the halo units with the exception of instanced geometry
            #this scales up to match the rest of the geometry and corrects the translation to match the unit settings
            offset_scale = self.get_scale_multiplier() / 0.01
            pos, rot, scale = perm.transform.decompose()
            return Matrix.Translation(pos * offset_scale) @ rot.to_matrix().to_4x4() @ Matrix.Diagonal(scale).to_4x4() @ Matrix.Scale(100 * perm.scale, 4)
        else:
            return Matrix()

###############################################################################################################################

def clean_scene():
    
    for item in bpy.data.objects:
        if item.type == 'MESH' or item.type == 'EMPTY':
            bpy.data.objects.remove(item)
            
    check_users = False
    for collection in (bpy.data.meshes, bpy.data.armatures, bpy.data.materials, bpy.data.textures, bpy.data.images):
        for item in collection:
            if item.users == 0 or not check_users:
                collection.remove(item)

def main(context, import_filename, options):
    print("execute amf import")

    IMPORT_SCALE = options.get_scale_multiplier()
    print(f"importing at scale {IMPORT_SCALE}")

    bitmapsDir = ospath.split(import_filename)[0]
    if len(options.DIRECTORY_BITMAP) > 0:
        bitmapsDir = options.DIRECTORY_BITMAP

    reader = FileReader(import_filename)

    model = AmfModel(reader)

    arm_obj = None
    armature = None
    model_nodes = []
    model_materials = []

    if options.IMPORT_BONES and len(model.nodes) > 0:
        print("creating bones")
        bpy.ops.object.add(type = 'ARMATURE', enter_editmode = True)
        arm_obj = context.object
        armature = arm_obj.data
        armature.name = model.name + " armature"

        node_abs_transforms = options.get_node_transforms(model.nodes, 0.5) #why is this getting doubled somehow?
        for i, node in enumerate(model.nodes):
            bone = armature.edit_bones.new(options.PREFIX_BONE + node.name)
            if node.parent_index >= 0:
                bone.parent = model_nodes[node.parent_index]
            bone.tail = Vector([1,0,0]) * IMPORT_SCALE

            #'bone.transform' only applies rotation
            bone.transform(node_abs_transforms[i], scale = False)
            bone.translate(node_abs_transforms[i].translation)
            model_nodes.append(bone)

        bpy.ops.object.mode_set(mode = 'OBJECT')

    if options.IMPORT_MARKERS and len(model.markers) > 0:
        print("creating markers")
        for mg in model.markers:
            marker_name = options.PREFIX_MARKER + mg.name
            node_abs_transforms = options.get_node_transforms(model.nodes)
            for m in mg.markers:
                if options.MODE_MARKERS == 'MESH':
                    mesh = bpy.data.meshes.new(marker_name)
                    marker_obj = bpy.data.objects.new(marker_name, mesh)

                    bm = bmesh.new()
                    bmesh.ops.create_uvsphere(bm, u_segments = 16, v_segments = 16, diameter = IMPORT_SCALE)
                    bm.to_mesh(mesh)
                    bm.free()

                    context.scene.collection.objects.link(marker_obj)
                else: # MODE_MARKERS == 'EMPTY'
                    #it says 'radius' but it comes out the same size as uvsphere's diameter
                    bpy.ops.object.empty_add(type = 'SPHERE', radius = IMPORT_SCALE)
                    marker_obj = context.object
                    marker_obj.name = marker_name

                marker_transform = Matrix.Translation(m.position * IMPORT_SCALE) @ m.rotation.to_matrix().to_4x4()
                if m.node_index >= 0:
                    marker_transform = node_abs_transforms[m.node_index] @ marker_transform
                    if options.IMPORT_BONES:
                        marker_obj.parent = arm_obj
                        marker_obj.parent_type = 'BONE'
                        marker_obj.parent_bone = options.PREFIX_BONE + model.nodes[m.node_index].name

                marker_obj.hide_render = True
                marker_obj.matrix_world = marker_transform

    if options.IMPORT_MATERIALS and len(model.materials) > 0:
        print("creating materials")
        for mat in model.materials:
            material = bpy.data.materials.new(mat.name)
            material.use_nodes = True

            bsdf = material.node_tree.nodes["Principled BSDF"]

            texture = material.node_tree.nodes.new('ShaderNodeTexImage')
            texture_path = mat.textures[0].get_full_path(bitmapsDir, options.SUFFIX_BITMAP)
            if not mat.textures[0].is_null and texture_path.exists():
                texture.image = bpy.data.images.load(str(texture_path))
                if not mat.is_transparent:
                    texture.image.alpha_mode = 'CHANNEL_PACKED'
                    material.node_tree.links.new(bsdf.inputs['Specular'], texture.outputs['Alpha'])

            material.node_tree.links.new(bsdf.inputs['Base Color'], texture.outputs['Color'])
            bsdf.location = [0, 300]
            texture.location = [-300, 300]

#           texture.repeat_x = mat.textures[0].uv_tiles[0]
#           texture.repeat_y = mat.textures[0].uv_tiles[1]

            if not mat.is_terrain and not mat.textures[3].is_null:
                texture = material.node_tree.nodes.new('ShaderNodeTexImage')
                texture_path = mat.textures[3].get_full_path(bitmapsDir, options.SUFFIX_BITMAP)
                if texture_path.exists():
                    texture.image = bpy.data.images.load(str(texture_path))
                    texture.image.alpha_mode = 'NONE'
                    texture.image.colorspace_settings.name = 'Non-Color'
                    
                # inverting green channel is necessary to convert from DirectX coordsys to OpenGL
                
                bump = material.node_tree.nodes.new('ShaderNodeNormalMap')
                split = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                invert = material.node_tree.nodes.new('ShaderNodeInvert')
                merge = material.node_tree.nodes.new('ShaderNodeCombineRGB')
                
                texture.location = [-1100, -100]
                split.location = [-800, -100]
                invert.location = [-600, -200]
                merge.location = [-400, -100]
                bump.location = [-200, -100]
                
                material.node_tree.links.new(split.inputs['Image'], texture.outputs['Color'])
                material.node_tree.links.new(invert.inputs['Color'], split.outputs['G'])
                material.node_tree.links.new(merge.inputs['R'], split.outputs['R'])
                material.node_tree.links.new(merge.inputs['G'], invert.outputs['Color'])
                material.node_tree.links.new(merge.inputs['B'], split.outputs['B'])
                material.node_tree.links.new(bump.inputs['Color'], merge.outputs['Image'])
                material.node_tree.links.new(bsdf.inputs['Normal'], bump.outputs['Normal'])
 
            model_materials.append(material)

    if options.IMPORT_MESHES and len(model.regions) > 0:
        print("creating meshes")
        instance_lookup = dict()
        for region in model.regions:
            for perm in region.permutations:
                index_buffer = model.index_buffers[perm.face_address]
                position_buffer = [v.position for v in model.vertex_buffers[perm.vert_address]]
                texcoord_buffer = [v.texcoords for v in model.vertex_buffers[perm.vert_address]]
                normal_buffer = [v.normal for v in model.vertex_buffers[perm.vert_address]]
                node_index_buffer = [v.indices for v in model.vertex_buffers[perm.vert_address]]
                node_weight_buffer = [v.weights for v in model.vertex_buffers[perm.vert_address]]

                for sub_index, sub in enumerate(perm.submeshes):
                    instance_key = f"{perm.vert_address}_{sub_index:03d}"
                    root_obj = instance_lookup.get(instance_key, None)
                    if root_obj is not None:
                        mesh_obj = root_obj.copy()
                        #this makes a deep copy rather than a linked copy
                        #mesh_obj.data = root_obj.data.copy()
                        mesh_obj.matrix_world = options.get_perm_transform(perm)
                        context.scene.collection.objects.link(mesh_obj)

                        if options.MODE_MESHES == 'JOIN':
                            break #skip the rest of the submeshes
                        else:
                            continue

                    if options.MODE_MESHES == 'JOIN':
                        mesh_name = f"{region.name}:{perm.name}"
                        mesh_verts = position_buffer
                        mesh_faces = index_buffer
                        mesh_coords = [texcoord_buffer[i] for i in itertools.chain(*mesh_faces)] #blender wants 3 uvs per tri, rather than one per vertex
                        mesh_norms = normal_buffer
                        mesh_nodes = node_index_buffer
                        mesh_weights = node_weight_buffer
                        mat_ranges = [(s.face_start, s.face_count, s.mat_index) for s in perm.submeshes]
                    else:
                        mesh_name = f"{region.name}:{perm.name}.{sub_index:03d}"
                        mesh_faces = [index_buffer[i] for i in range(sub.face_start, sub.face_start + sub.face_count)]
                        mesh_coords = [texcoord_buffer[i] for i in itertools.chain(*mesh_faces)] #blender wants 3 uvs per tri, rather than one per vertex

                        min_index = min(itertools.chain(*mesh_faces))
                        max_index = max(itertools.chain(*mesh_faces))

                        mesh_verts = [position_buffer[i] for i in range(min_index, max_index + 1)] #only need verts referenced by the current faces
                        mesh_norms = [normal_buffer[i] for i in range(min_index, max_index + 1)]

                        if perm.vert_format > 0:
                            mesh_nodes = [node_index_buffer[i] for i in range(min_index, max_index + 1)]
                            mesh_weights = [node_weight_buffer[i] for i in range(min_index, max_index + 1)]

                        mat_ranges = [(0, sub.face_count, sub.mat_index)] #split meshes are offset to always start at 0

                        # translate the face indices to start at 0 since we are only using a subset of the vertices now
                        for face in mesh_faces:
                            face[0] -= min_index
                            face[1] -= min_index
                            face[2] -= min_index

                    mesh = bpy.data.meshes.new(mesh_name)
                    mesh.from_pydata(mesh_verts, [], mesh_faces)
                    mesh_obj = bpy.data.objects.new(mesh_name, mesh)

                    mesh.normals_split_custom_set_from_vertices(mesh_norms)
                    mesh.use_auto_smooth = True #required for custom normals to take effect

                    if options.IMPORT_MATERIALS:
                        mat_lookup = dict()
                        for i, mat_index in enumerate(set(t[2] for t in mat_ranges)):
                            mat_lookup[mat_index] = i

                        for i in mat_lookup.keys():
                            mesh.materials.append(model_materials[i])

                        for face_start, face_count, mat_index in mat_ranges:
                            for i in range(face_start, face_start + face_count):
                                mesh.polygons[i].material_index = mat_lookup[mat_index]

                    uvmap = mesh.uv_layers.new()
                    for i in range(len(mesh_coords)):
                        uvmap.data[i].uv = mesh_coords[i]

                    if options.IMPORT_BONES and perm.vert_format > 0:
                        mesh_obj.parent = arm_obj
                        mod = mesh_obj.modifiers.new("armature", 'ARMATURE')
                        mod.object = arm_obj

                        for node in model.nodes:
                            mesh_obj.vertex_groups.new(name = node.name)

                        if perm.node_index != 255:
                            for i in range(len(mesh_verts)):
                                mesh_obj.vertex_groups[perm.node_index].add([i], 1.0, 'ADD')
                        else:
                            for i in range(len(mesh_nodes)):
                                indices = mesh_nodes[i]
                                weights = mesh_weights[i]
                                for j in range(len(indices)):
                                    if weights[j] == 0:
                                        continue
                                    mesh_obj.vertex_groups[int(indices[j])].add([i], weights[j], 'ADD')

                    if not math.isnan(perm.scale):
                        mesh_obj.matrix_world = options.get_perm_transform(perm)
                        instance_lookup[instance_key] = mesh_obj

                    mesh.transform(Matrix.Scale(IMPORT_SCALE, 4))
                    context.scene.collection.objects.link(mesh_obj)

                    if options.MODE_MESHES == 'JOIN':
                        break #skip the rest of the submeshes

    reader.close()

###############################################################################################################################

class IMPORT_SCENE_OT_amf(bpy.types.Operator, bpy_extras.io_utils.ImportHelper):
    """Import an AMF file"""
    bl_idname = "import_scene.amf"
    bl_label = "Import AMF"
    bl_options = {'PRESET', 'UNDO'}

    filename_ext = ".amf"
    filter_glob: StringProperty(
        default = "*.amf",
        options = {'HIDDEN'},
    )

    import_units: EnumProperty(
        name = "Units",
        items = (
            ('METERS', "Meters", "Import using meters"),
            ('HALO', "Halo", "Import using Halo's world units"),
            ('MAX', "3DS Max", "Import using 3DS Max units (100x Halo)"),
        )
    )

    import_scale: FloatProperty(
        name = "Multiplier",
        description = "Sets the size of the imported model",
        default = 1.0,
        min = 0.0
    )

    import_nodes: BoolProperty(
        name = "Import Bones",
        description = "Determines if an armature and bones will be created, when applicable",
        default = True
    )

    node_prefix: StringProperty(
        name = "Prefix",
        description = "Adds a custom prefix in front of bone names",
        default = ""
    )

    import_markers: BoolProperty(
        name = "Import Markers",
        description = "Determines if marker spheres will be created, when applicable",
        default = True
    )

    marker_mode: EnumProperty(
        name = "Type",
        items = (
            ('EMPTY', "Empty Sphere", "Create markers using empty spheres"),
            ('MESH', "Mesh Sphere", "Create markers using UVSpheres"),
        )
    )

    marker_prefix: StringProperty(
        name = "Prefix",
        description = "Adds a custom prefix in front of marker names",
        default = "#"
    )

    import_meshes: BoolProperty(
        name = "Import Meshes",
        description = "Determines if mesh geometry will be created",
        default = True
    )

    mesh_mode: EnumProperty(
        name = "Group by",
        items = (
            ('JOIN', "Permutation", "Create one mesh per permutation"),
            ('SPLIT', "Material", "Create one mesh per material index"),
        )
    )

    import_materials: BoolProperty(
        name = "Import Materials",
        description = "Determines if materials will be created and applied",
        default = True
    )

    bitmap_dir: StringProperty(
        name = "Folder",
        description = "The root folder where bitmaps are saved",
        default = "",
#        subtype = 'DIR_PATH' # blender will not allow us to open a browser dilaog while the import dialog is open
    )

    bitmap_ext: StringProperty(
        name = "Extension",
        description = "The file extension of the source bitmap files",
        default = "tif"
    )

    def execute(self, context):
        options = ImportOptions()
        options.IMPORT_SCALE = self.import_scale
        options.IMPORT_BONES = self.import_nodes
        options.IMPORT_MARKERS = self.import_markers
        options.IMPORT_MESHES = self.import_meshes
        options.IMPORT_MATERIALS = self.import_materials

        options.PREFIX_MARKER = self.marker_prefix
        options.PREFIX_BONE = self.node_prefix

        options.DIRECTORY_BITMAP = self.bitmap_dir
        options.SUFFIX_BITMAP = self.bitmap_ext.lstrip(" .").rstrip()

        options.MODE_SCALE = self.import_units
        options.MODE_MARKERS = self.marker_mode
        options.MODE_MESHES = self.mesh_mode

        main(context, self.filepath, options)

        return {'FINISHED'}

    def draw(self, context):
        layout = self.layout

        box = layout.box()
        box.label(icon = 'WORLD_DATA', text = "Scale")
        box.prop(self, "import_units")
        box.prop(self, "import_scale")

        box = layout.box()
        row = box.row()
        row.label(icon = 'BONE_DATA', text = "Bones")
        row.prop(self, "import_nodes")
        if self.import_nodes:
            box.prop(self, "node_prefix")

        box = layout.box()
        row = box.row()
        row.label(icon = 'MESH_CIRCLE', text = "Markers")
        row.prop(self, "import_markers")
        if self.import_markers:
            box.prop(self, "marker_mode")
            box.prop(self, "marker_prefix")

        box = layout.box()
        row = box.row()
        row.label(icon = 'MATERIAL_DATA', text = "Materials")
        row.prop(self, "import_materials")
        if self.import_materials:
            box.prop(self, "bitmap_dir")
            box.prop(self, "bitmap_ext")

        box = layout.box()
        row = box.row()
        row.label(icon = 'MESH_DATA', text = "Meshes")
        row.prop(self, "import_meshes")
        if self.import_meshes:
            box.prop(self, "mesh_mode")

# dont seem to be able to have a different value for menu label/dialog label within a single operator
# work around this by having one operator for the menu, and one for the dialog
class IMPORT_SCENE_MT_amf(bpy.types.Operator):
    """Import an AMF file"""
    bl_idname = "menu_import.amf"
    bl_label = "AMF (.amf)"

    def execute(self, context):
        bpy.ops.import_scene.amf('INVOKE_DEFAULT')
        return {'FINISHED'}

def draw_operator(self, context):
    self.layout.operator(IMPORT_SCENE_MT_amf.bl_idname)

def register():
    bpy.utils.register_class(IMPORT_SCENE_OT_amf)
    bpy.utils.register_class(IMPORT_SCENE_MT_amf)
    bpy.types.TOPBAR_MT_file_import.append(draw_operator)

def unregister():
    bpy.types.TOPBAR_MT_file_import.remove(draw_operator)
    bpy.utils.unregister_class(IMPORT_SCENE_MT_amf)
    bpy.utils.unregister_class(IMPORT_SCENE_OT_amf)

###############################################################################################################################

if __name__ == "__main__":
    try:
        unregister()
    finally:
        clean_scene()
        register()    
        bpy.ops.import_scene.amf('INVOKE_DEFAULT')