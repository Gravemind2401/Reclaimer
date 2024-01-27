import unittest
from ..src.SceneReader import SceneReader

class Test_Citadel(unittest.TestCase):
    def test_citadel(self):
        scene = SceneReader.open_scene('Z:\\data\\100_citadel.rmf')
        return

class Test_Brute(unittest.TestCase):
    def test_brute(self):
        scene = SceneReader.open_scene('Z:\\data\\brute.rmf')
        return

class Test_Masterchief(unittest.TestCase):
    def test_masterchief(self):
        scene = SceneReader.open_scene('Z:\\data\\masterchief.rmf')
        for buf in scene.vertex_buffer_pool:
            for channel in buf.position_channels:
                for _ in channel:
                    pass
            for channel in buf.normal_channels:
                for _ in channel:
                    pass
            for channel in buf.texcoord_channels:
                for _ in channel:
                    pass
        return

if __name__ == '__main__':
    unittest.main()
