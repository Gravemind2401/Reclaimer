import unittest
from ..src.SceneReader import SceneReader

class Test_Citadel(unittest.TestCase):
    def test_citadel(self):
        sr = SceneReader()
        sr.read_scene('Z:\\data\\100_citadel.rmf')

class Test_Brute(unittest.TestCase):
    def test_brute(self):
        sr = SceneReader()
        sr.read_scene('Z:\\data\\brute.rmf')

class Test_Masterchief(unittest.TestCase):
    def test_masterchief(self):
        sr = SceneReader()
        sr.read_scene('Z:\\data\\masterchief.rmf')

if __name__ == '__main__':
    unittest.main()
