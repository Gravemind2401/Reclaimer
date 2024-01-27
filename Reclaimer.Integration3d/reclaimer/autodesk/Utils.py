from pymxs import runtime
from typing import Union

from ..src.Types import *

# type alias shortcuts

Point2 = runtime.Point2
Point3 = runtime.Point3
Point4 = runtime.Point4
Quat = runtime.Quat

Matrix3 = runtime.Matrix3

BitArray = runtime.BitArray

MaxOps = runtime.MaxOps
BoneSys = runtime.BoneSys
SkinOps = runtime.SkinOps

# util functions

def toPoint2(value: Union[Float2, Float3, Float4]) -> Point2:
    ''' Creates a 3dsMax Point2 from a float collection '''
    return Point2(value[0], value[1])

def toPoint3(value: Union[Float3, Float4]) -> Point3:
    ''' Creates a 3dsMax Point3 from a float collection '''
    return Point3(value[0], value[1], value[2])
        
def toPoint4(value: Float4) -> Point4:
    ''' Creates a 3dsMax Point4 from a float collection '''
    return Point4(value[0], value[1], value[2], value[3])
        
def toQuat(value: Float4) -> Quat:
    ''' Creates a 3dsMax Quat from a float collection '''
    return Quat(value[0], value[1], value[2], value[3])

def toMatrix3(mat: Matrix4x4) -> Matrix3:
    ''' Creates a 3dsMax Matrix3 from a 4x4 float collection '''
    rows = [toPoint3(row) for row in mat]
    return Matrix3(*rows)