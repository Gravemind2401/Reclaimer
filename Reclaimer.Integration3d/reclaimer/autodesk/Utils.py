from pymxs import runtime as rt
from typing import Union

from ..src.Types import *


def toPoint2(value: Union[Float2, Float3, Float4]) -> rt.Point2:
    ''' Creates a 3dsMax Point2 from a float collection '''
    return rt.Point2(value[0], value[1])

def toPoint3(value: Union[Float3, Float4]) -> rt.Point3:
    ''' Creates a 3dsMax Point3 from a float collection '''
    return rt.Point3(value[0], value[1], value[2])
        
def toPoint4(value: Float4) -> rt.Point4:
    ''' Creates a 3dsMax Point4 from a float collection '''
    return rt.Point4(value[0], value[1], value[2], value[3])
        
def toQuat(value: Float4) -> rt.Quat:
    ''' Creates a 3dsMax Quat from a float collection '''
    return rt.Quat(value[0], value[1], value[2], value[3])

def toMatrix3(value: Matrix4x4) -> rt.Matrix3:
    ''' Creates a 3dsMax Matrix3 from a 4x4 float collection '''
    rows = [toPoint3(row) for row in value]
    return rt.Matrix3(*rows)

def toColor(value: ColorF) -> rt.Color:
    ''' Creates a 3dsMax Color from a float collection representing RGBA values in the range of 0.0 to 1.0 '''
    return rt.Color(255.0 * value[0], 255.0 * value[1], 255.0 * value[2], 255.0 * value[3])