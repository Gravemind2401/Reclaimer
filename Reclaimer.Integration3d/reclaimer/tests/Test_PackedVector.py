import unittest
from ..src.vectors.PackedVector import PackedVector, DecN4, UDecN4, UHenDN3

class Test_PackedVector(unittest.TestCase):
    def test_DecN4(self):
        vec = PackedVector(0x00000000, DecN4)
        self.assertEqual(repr(vec), '[0.000000, 0.000000, 0.000000, 0.000000]')
        
        vec = PackedVector(0x00000F00, DecN4)
        self.assertEqual(repr(vec), '[-0.500978, 0.005871, 0.000000, 0.000000]')
        
        vec = PackedVector(0x00FFF000, DecN4)
        self.assertEqual(repr(vec), '[0.000000, -0.007828, 0.029354, 0.000000]')
        
        vec = PackedVector(0xF00F00FF, DecN4)
        self.assertEqual(repr(vec), '[0.499022, -0.125245, -0.500978, -1.000000]')
        
        vec = PackedVector(0xFFF00F00, DecN4)
        self.assertEqual(repr(vec), '[-0.500978, 0.005871, -0.001957, -1.000000]')
        
        vec = PackedVector(0xFFFFFFFF, DecN4)
        self.assertEqual(repr(vec), '[-0.001957, -0.001957, -0.001957, -1.000000]')


    def test_UDecN4(self):
        vec = PackedVector(0x00000000, UDecN4)
        self.assertEqual(repr(vec), '[0.000000, 0.000000, 0.000000, 0.000000]')
        
        vec = PackedVector(0x00000F00, UDecN4)
        self.assertEqual(repr(vec), '[0.750733, 0.002933, 0.000000, 0.000000]')
        
        vec = PackedVector(0x00FFF000, UDecN4)
        self.assertEqual(repr(vec), '[0.000000, 0.997067, 0.014663, 0.000000]')
        
        vec = PackedVector(0xF00F00FF, UDecN4)
        self.assertEqual(repr(vec), '[0.249267, 0.938416, 0.750733, 1.000000]')
        
        vec = PackedVector(0xFFF00F00, UDecN4)
        self.assertEqual(repr(vec), '[0.750733, 0.002933, 1.000000, 1.000000]')
        
        vec = PackedVector(0xFFFFFFFF, UDecN4)
        self.assertEqual(repr(vec), '[1.000000, 1.000000, 1.000000, 1.000000]')


    def test_UHenDN3(self):
        vec = PackedVector(0x00000000, UHenDN3)
        self.assertEqual(repr(vec), '[0.000000, 0.000000, 0.000000]')
        
        vec = PackedVector(0x00000F00, UHenDN3)
        self.assertEqual(repr(vec), '[0.875427, 0.000489, 0.000000]')
        
        vec = PackedVector(0x00FFF000, UHenDN3)
        self.assertEqual(repr(vec), '[0.000000, 0.999511, 0.002933]')
        
        vec = PackedVector(0xF00F00FF, UHenDN3)
        self.assertEqual(repr(vec), '[0.124573, 0.234489, 0.938416]')
        
        vec = PackedVector(0xFFF00F00, UHenDN3)
        self.assertEqual(repr(vec), '[0.875427, 0.750855, 1.000000]')
        
        vec = PackedVector(0xFFFFFFFF, UHenDN3)
        self.assertEqual(repr(vec), '[1.000000, 1.000000, 1.000000]')

if __name__ == '__main__':
    unittest.main()
