/******************************************************************************
AMF Importer By Gravemind2401
-------------------------------------------------------------------------------
A new format built by Gravemind2401 to cater for many, many more options 
when importing as well as up to 85%+ smaller filesize and better shader support
******************************************************************************/

header = 0
version = 0.0
mName = ""

inFile = ""
loaded = false

nodeInfo = #()
regionInfo = #()
shaderInfo = #()
shaderTypes = #()
boneList = #()
meshList = #()
treeNodes = #()
usedShaders = #()

mGroupInfo = #()

fn readHeader = (
	header = readLong inFile
	version = readFloat inFile
	mName = readString inFile
)

fn readNodes = (
	nodeInfo = #()
	
	nCount = readLong inFile
	nAddress = readLong inFile
	fPos = ftell inFile
	
	if nCount > 0 do (
		fseek inFile nAddress #seek_set
		
		for n = 1 to nCount do (
			nName = readString inFile
			parentIndex = readShort inFile
			childIndex = readShort inFile
			siblingIndex = readShort inFile
			pos = [readFloat inFile, readFloat inFile, readFloat inFile]
			rot = quat (readFloat inFile) (readFloat inFile) (readFloat inFile) (readFloat inFile)
			
			--in case max decides to order by name
			pre = ""
			if n < 100 do ( pre += "0" )
			if n < 010 do ( pre += "0" )
			nName = (pre + (n as string) + nName)
			
			append nodeInfo #(nName, parentIndex, childIndex, siblingIndex, pos, rot)
		)
	)
	fseek inFile fPos #seek_set
)

fn readMarkers = (
	mGroupInfo = #()
	
	mgCount = readLong inFile
	mgAddress = readLong inFile
	fPos = ftell inFile
	
	if mgCount > 0 do (
		fseek inFile mgAddress #seek_set
		
		for mg = 1 to mgCount do (
			markers = #()
			
			mgName = "#" + readString inFile
			mCount = readLong inFile
			mAddress = readLong inFile
			cPos = ftell inFile
			
			if mCount > 0 do (
				fseek inFile mAddress #seek_set
				
				for m = 1 to mCount do (
					rIndex = readByte inFile
					pIndex = readByte inFile
					nIndex = readShort inFile
					pos = [readFloat inFile, readFloat inFile, readFloat inFile]
					rot = quat (readFloat inFile) (readFloat inFile) (readFloat inFile) (readFloat inFile)
					append markers #(rIndex, pIndex, nIndex, pos, rot)
				)
			)
			append mGroupInfo #(mgName, markers)
			
			fseek inFile cPos #seek_set
		)
	)
	fseek inFile fPos #seek_set
)

fn getVMatrix xbounds ybounds zbounds = (
	dMat = matrix3 [1.0 / 65535.0, 0, 0] [0, 1.0 / 65535.0, 0] [0, 0, 1.0 / 65535.0] [0, 0, 0]
	bMat = matrix3 [xbounds.y - xbounds.x, 0, 0] [0, ybounds.y - ybounds.x, 0] [0, 0, zbounds.y - zbounds.x] [xbounds.x, ybounds.x, zbounds.x]
	--return (dMat * bMat)
	return dMat
)

fn getTMatrix ubounds vbounds = (
	dMat = matrix3 [1.0 / 32767.0, 0, 0] [0, 1.0 / 32767.0, 0] [0, 0, 1.0 / 32767.0] [0, 0, 0]
	bMat = matrix3 [ubounds.y - ubounds.x, 0, 0] [0, vbounds.y - vbounds.x, 0] [0, 0, 0] [ubounds.x, vbounds.x, 0]
	--return (dMat * bMat)
	return matrix3 1
)

fn readVertices vFormat cFormat vCount = (
	verts = #()
	vmat = matrix3 1
	tmat = matrix3 1
	
	if vCount > 0 do (
		if cFormat > 0 then (
			xbounds = [readFloat inFile, readFloat inFile]
			ybounds = [readFloat inFile, readFloat inFile]
			zbounds = [readFloat inFile, readFloat inFile]
			ubounds = [readFloat inFile, readFloat inFile]
			vbounds = [readFloat inFile, readFloat inFile]
			
			vmat = getVMatrix xbounds ybounds zbounds
			tmat = getTMatrix ubounds vbounds
		)
		
		for v = 1 to vCount do (
			if cFormat == 1 then (
				pos = [readShort inFile, readShort inFile, readShort inFile]
				norm = [0,0,0]
				readLong inFile
				tex = [readShort inFile, readShort inFile, 0.0] * tmat
			)
			else (
				pos = [readFloat inFile, readFloat inFile, readFloat inFile]
				norm = [readFloat inFile, readFloat inFile, readFloat inFile]
				tex = [readFloat inFile, readFloat inFile, 0.0]
			)
			indices = #()
			weights = #()
			
			if vFormat == 1 do (
				iCount = 1
				i1 = (readByte inFile #unsigned) + 1
				i2 = (readByte inFile #unsigned) + 1
				if(i2 != 256) do (
					iCount += 1
					i3 = (readByte inFile #unsigned) + 1
					
					if(i3 != 256) do (
						iCount += 1
						i4 = (readByte inFile #unsigned) + 1
						
						if(i4 != 256) do (
							iCount += 1
						)
					)
				)
				
				w1 = readFloat inFile
				
				indices = #(i1)
				weights = #(w1)
				
				if(iCount > 1) do (
					w2 = readFloat inFile
					indices = #(i1, i2)
					weights = #(w1, w2)
				)
				if(iCount > 2) do (
					w3 = readFloat inFile
					indices = #(i1, i2, i3)
					weights = #(w1, w2, w3)
				)
				if(iCount > 3) do (
					w4 = readFloat inFile
					indices = #(i1, i2, i3, i4)
					weights = #(w1, w2, w3, w4)
				)
			)
			
			if vFormat == 2 do (
				i1 = (readByte inFile #unsigned) + 1
				indices = #(i1)
				weights = #(1.0)
				i2 = (readByte inFile #unsigned) + 1
				
				if(i2 != 256) do (
					indices = #(i1, i2)
					weights = #(1.0, 1.0)
					i3 = (readByte inFile #unsigned) + 1
					
					if(i3 != 256) do (
						indices = #(i1, i2, i3)
						weights = #(1.0, 1.0, 1.0)
						i4 = (readByte inFile #unsigned) + 1
						
						if(i4 != 256) do (
							indices = #(i1, i2, i3, i4)
							weights = #(1.0, 1.0, 1.0, 1.0)
						)
					)
				)
			)
			append verts #(pos, norm, tex, indices, weights)
		)
	)

	return #(verts, vmat)
)

fn copyVertices vFrom cFormat  = (
	verts = #()
	vmat = matrix3 1
	tmat = matrix3 1
	
	if vFrom.count > 0 do (
		if cFormat > 0 then (
			xbounds = [readFloat inFile, readFloat inFile]
			ybounds = [readFloat inFile, readFloat inFile]
			zbounds = [readFloat inFile, readFloat inFile]
			ubounds = [readFloat inFile, readFloat inFile]
			vbounds = [readFloat inFile, readFloat inFile]
			
			vmat = getVMatrix xbounds ybounds zbounds
			tmat = getTMatrix ubounds vbounds
		)
	)

	return #(vFrom, vmat)
)

fn readFaces vLen fCount = (
	faces = #()
	if fCount > 0 do (
		for f = 1 to fCount do (
			if vLen > 65535 then (
				append faces [readLong inFile + 1, readLong inFile + 1, readLong inFile + 1]
			)
			else (
				append faces  [readShort inFile #unsigned + 1, readShort inFile #unsigned + 1, readShort inFile #unsigned + 1]
			)
		)
	)
	return faces
)

fn readMeshes sCount = (
	meshes = #()
	if sCount > 0 do (
		for s = 1 to sCount do (
			shIndx = readShort inFile + 1
			fStart = readLong inFile + 1
			fCount = readLong inFile
			append meshes #(shIndx, fStart, fCount)
		)
	)
	return meshes
)

fn readRegions = (
	regionInfo = #()
	
	rCount = readLong inFile
	rAddress = readLong inFile
	fPos = ftell inFile
	
	if rCount > 0 do (
		fseek inFile rAddress #seek_set
		
		for r = 1 to rCount do (
			perms = #()
			
			rName = readString inFile
			pCount = readLong inFile
			pAddress = readLong inFile
			pPos = ftell inFile
			
			if pCount > 0 do (
				fseek inFile pAddress #seek_set
				
				for p = 1 to pCount do (
					pName = readString inFile
					vTemp = readByte inFile
					nIndex = readByte inFile #unsigned
					vCount = readLong inFile
					vAddress = readLong inFile
					fCount = readLong inFile
					fAddress = readLong inFile
					sCount = readLong inFile
					sAddress = readLong inFile
					
					vFormat = bit.and vTemp 15
					cFormat = bit.shift (bit.and vTemp 240) -4
					
					trnsfm = matrix3 1					
					if version >= 0.1 do (
						mult = readFloat inFile
						if not bit.isNAN mult do (
							row1 = [readFloat inFile, readFloat inFile, readFloat inFile]
							row2 = [readFloat inFile, readFloat inFile, readFloat inFile]
							row3 = [readFloat inFile, readFloat inFile, readFloat inFile]
							row4 = [readFloat inFile, readFloat inFile, readFloat inFile]
							
							trnsfm = matrix3 row1 row2 row3 row4
						)
					)
					
					xPos = ftell inFile
					
					vDone = false
					fDone = false
					
					verts = #()
					faces = #()
					
					--save memory, use same vertices as before if possible
					for permX in perms do (
						if permX[7] == vAddress and not vDone do ( 
							verts = copyVertices permX[4] cFormat
							vDone = true
						)
						if permX[8] == fAddress and not fDone do ( 
							faces = permX[5] 
							fDone = true
						)
						if vDone and fDone do ( exit )
					)
					
					if not vDone do (
						fseek inFile vAddress #seek_set
						verts = readVertices vFormat cFormat vCount
					)
					
					if not fDone do (
						fseek inFile fAddress #seek_set
						faces = readFaces vCount fCount
					)
					
					fseek inFile sAddress #seek_set
					meshes = readMeshes sCount
					
					print ("verts: " + (verts[2] as string))
					print ("trnsfm: " + (trnsfm as string))
					append perms #(pName, vFormat, nIndex, verts[1], faces, meshes, vAddress, fAddress, mult, (verts[2] * trnsfm))
					
					fseek inFile xPos #seek_set
				)
			)
			append regionInfo #(rName, perms)
			
			fseek inFile pPos #seek_set
		)
	)
	fseek inFile fPos #seek_set
)

fn readShaders = (
	shaderInfo = #()
	shaderTypes = #()
	
	sCount = readLong inFile
	sAddress = readLong inFile
	fPos = ftell inFile
	
	if sCount > 0 do (
		fseek inFile sAddress #seek_set
		
		for s = 1 to sCount do (
			sName = readString inFile
			sType = 0
			
			--* denotes a terrain blend shader
			if sName[1] == "*" do (
				sType = 1
				sName = (substring sName 2 (sName.count - 1))
			)
			
			append shaderTypes sType
			
			if sType == 0 then (
				paths = #()
				uvTiles = #()
				tints = #()
				for i = 1 to 8 do (
					append paths (readString inFile)
					if paths[i] != "null" then (
						append uvTiles [readFloat inFile, readFloat inFile]
					)
					else (
						append uvTiles [0.0, 0.0]
					)
				)
				
				if version >= 1.1 then (
					for i = 1 to 4 do (
						append tints [readByte inFile #unsigned, readByte inFile #unsigned, readByte inFile #unsigned, readByte inFile #unsigned]
					)
				)
				else (
					for i = 1 to 4 do (
						append tints [0, 0, 0, 0]
					)
				)
				
				isTrans = (readByte inFile == 1)
				ccOnly = (readByte inFile == 1)
				append shaderInfo #(sName, paths, uvTiles, isTrans, ccOnly, tints)
			)
			else (
				baseMaps = #()
				bumpMaps = #()
				detMaps = #()
				
				baseTiles = #()
				bumpTiles = #()
				detTiles = #()
				
				blendPath = readString inFile
				if blendPath != "null" then (
					blendTile = [readFloat inFile, readFloat inFile]
				)
				else (
					blendTile = [0, 0]
				)
				
				baseCount = readByte inFile
				bumpCount = readByte inFile
				detCount = readByte inFile
				
				for i = 1 to baseCount do (
					append baseMaps (readString inFile)
					append baseTiles [readFloat inFile, readFloat inFile]
				)
				
				for i = 1 to bumpCount do (
					append bumpMaps (readString inFile)
					append bumpTiles [readFloat inFile, readFloat inFile]
				)
				
				for i = 1 to detCount do (
					append detMaps (readString inFile)
					append detTiles [readFloat inFile, readFloat inFile]
				)
				
				append shaderInfo #(sName, blendPath, blendTile, baseMaps, baseTiles, bumpMaps, bumpTiles, detMaps, detTiles)
			)
		)
	)
	fseek inFile fPos #seek_set
)

fn loadRegions tv = (
	treeNodes = #()
	
	for reg in regionInfo do (
		pNode = tv.Nodes.add reg[1] 
		pNode.checked = true
		subNodes = #()
		
		for perm in reg[2] do (
			cNode = pNode.Nodes.add perm[1]
			cNode.checked = true
			append subNodes cNode
		)
		
		append treeNodes #(pNode, subNodes)
	)
)

fn createBones nodeRadius boneEnable = (
	boneList = #()
	
	for b = 1 to nodeInfo.count do (
-- 		nodeInfo #(nName, parentIndex, childIndex, siblingIndex, pos, rot)
		iNode = nodeInfo[b]
		myBone = BoneSys.createBone iNode[5] [iNode[5].x + nodeRadius, iNode[5].y, iNode[5].z] [0,0,1]
		myBone.name = iNode[1]
		myBone.width = nodeRadius
		myBone.height = nodeRadius
		
		if iNode[3] == -1 then ( myBone.taper = 50 )
		else ( myBone.taper = 70 )
		
		mybone.setBoneEnable false 0
		append boneList myBone
		
		if iNode[2] != -1 do (
			myBone.parent = boneList[iNode[2] + 1]
		)
		
		in coordsys parent myBone.rotation = iNode[6]
		in coordsys parent myBone.pos = iNode[5]
	)
	
	for b = 1 to boneList.count do (
-- 		nodeInfo #(nName, parentIndex, childIndex, siblingIndex, pos, rot)
		myBone = boneList[b]
		nName = myBone.name as string
		if myBone.children.count > 0 then (
			for c = 1 to myBone.children.count do (
				dist = distance myBone myBone.children[c]
				if dist > myBone.length do ( myBone.length = dist )
			)
		)
		else (
			myBone.length = nodeRadius
		)
		
-- 		if (findString nName "pelvis" != undefined) or (findString nName "spine" != undefined) then (
-- 			myBone.height *= 0.5
-- 			myBone.taper = -180
-- 			if myBone.children.count > 0 do ( myBone.length = distance myBone myBone.children[1] )
-- 		)
-- 		
-- 		if (findString nName "l_thumb" != undefined) or (findString nName "r_thumb" != undefined) or 
-- 		(findString nName "l_index" != undefined) or (findString nName "r_index" != undefined) or 
-- 		(findString nName "l_middle" != undefined) or (findString nName "r_middle" != undefined) or 
-- 		(findString nName "l_ring" != undefined) or (findString nName "r_ring" != undefined) or 
-- 		(findString nName "l_pinky" != undefined) or (findString nName "r_pinky" != undefined) then (
-- 			myBone.height *= 0.2
-- 			myBone.width *= 0.2
-- 			
-- 			if(myBone.children.count == 0) do ( myBone.length *= 0.3 )
-- 		)
-- 		
-- 		if (findString nName "l_upperarm" != undefined) or (findString nName "r_upperarm" != undefined) or
-- 		(findString nName "l_thigh" != undefined) or (findString nName "r_thigh" != undefined) or
-- 		(findString nName "l_clav" != undefined) or (findString nName "r_clav" != undefined) then (
-- 			myBone.height *= 1.3
-- 			myBone.width *= 1.3
-- 		)
-- 		
-- 		if (findString nName "l_forearm" != undefined) or (findString nName "r_forearm" != undefined) or
-- 		(findString nName "l_calf" != undefined) or (findString nName "r_calf" != undefined) then (
-- 			myBone.height *= 1.15
-- 			myBone.width *= 1.15
-- 		)
-- 		
-- 		if (findString nName "l_hand" != undefined) or (findString nName "r_hand" != undefined) then (
-- 			myBone.height *= 0.2
-- 			myBone.length *= 0.75
-- 			myBone.taper = -75
-- 		)
-- 		
-- 		if (findString nName "l_foot" != undefined) or (findString nName "r_foot" != undefined) then (
-- 			myBone.length *= 0.65
-- 		)
-- 		
-- 		if (findString nName "l_cheek" != undefined) or (findString nName "r_cheek" != undefined) or 
-- 		(findString nName "l_brow" != undefined) or (findString nName "r_brow" != undefined) or 
-- 		(findString nName "l_lip" != undefined) or (findString nName "r_lip" != undefined) or 
-- 		(findString nName "l_nose" != undefined) or (findString nName "r_nose" != undefined) or 
-- 		(findString nName "l_hair" != undefined) or (findString nName "r_hair" != undefined) or 
-- 		(findString nName "c_lip" != undefined) or (findString nName "c_hair" != undefined) or 
-- 		(findString nName "l_mouth" != undefined) or (findString nName "r_mouth" != undefined) or 
-- 		(findString nName "eye_left" != undefined) or (findString nName "eye_right" != undefined) or 
-- 		(findString nName "l_eye" != undefined) or (findString nName "r_eye" != undefined) or 
-- 		(findString nName "jaw" != undefined) or (findString nName "f_nose" != undefined) or 
-- 		(findString nName "f_lip" != undefined) or (findString nName "f_thoat" != undefined) then (
-- 			myBone.height *= 0.2
-- 			myBone.width *= 0.2
-- 			myBone.length = nodeRadius * 0.2
-- 		)
-- 		
-- 		if (findString nName "helper" != undefined) or (findString nName "fixup" != undefined) or 
-- 		(findString nName "jiggle" != undefined) or (findString nName "pedestal" != undefined) or 
-- 		(findString nName "twist" != undefined) or (findString nName "twist" != undefined) then (
-- 			myBone.height = nodeRadius
-- 			myBone.width = nodeRadius
-- 			myBone.length = nodeRadius
-- 			myBone.taper = 50
-- 		)
		
		if boneEnable do (
			myBone.setBoneEnable true 0
		)
	)
	
	completeRedraw()
)

fn createMarkers markRadius = (
	for mg = 1 to mGroupInfo.count do(
		mGroup = mGroupInfo[mg][2]
		for m  = 1 to mGroup.count do(	
			marker = sphere radius:markRadius
			marker.name = mGroupInfo[mg][1]
			
			if mGroup[m][3] != -1 do (
				marker.parent = boneList[mGroup[m][3] + 1]
			)
			
			in coordsys parent marker.rotation = mGroup[m][5]
			in coordsys parent marker.pos = mGroup[m][4]
		)
	)
	completeRedraw()
)

fn createMeshes mrge norm weight uv unwrap tv = (
	meshList = #()
	usedShaders = #()
	meshDic = #()
	for r = 1 to treeNodes.count do (
		rNode = treeNodes[r]
		if(rNode[1].checked != true) do ( continue )
		
		rInfo = regionInfo[r]
		rName = rInfo[1]
		rPerms = rInfo[2]
		
		for p = 1 to rNode[2].count do (
			pNode = rNode[2][p]
			if(pNode.checked != true) do ( continue )
			
			pInfo = rPerms[p]
			pName = pInfo[1]
			vFormat = pInfo[2]
			nIndex = pInfo[3]
			
-- 			perms #(pName, vFormat, nIndex, verts, faces, meshes, vAddress, fAddress, trnsfm)
-- 			meshes #(shIndx, fStart, fCount)
-- 			verts #(pos, norm, tex, indices, weights)
			
			for s = 1 to pInfo[6].count do (
				if s > 1 and mrge do ( continue )
				
				sInfo = pInfo[6][s]
				found = false
				
				if version >= 0.1 do (
					if not bit.isNAN pInfo[9] do (
						for pair in meshDic do ( --check for an existing copy of an instance, and use that as a base
							if pair[1] == pInfo[7] do (
								maxOps.cloneNodes pair[2] cloneType:#instance newNodes:&newMesh
								
								if mrge then (
									newMesh.name = rName + ":" + pName
								)
								else (
									newMesh.name = rName + ":" + pName + ":" + (s as string)
								)
								
								mult = pInfo[9]
								
								newMesh.transform = (matrix3 [mult,0,0] [0,mult,0] [0,0,mult] [0,0,0])
								newMesh.transform *= pInfo[10]
								newMesh.transform *= (matrix3 [100,0,0] [0,100,0] [0,0,100] [0,0,0])
								
								append meshList newMesh
								found = true
							)
						)
					)
				)
				
				if found do continue
				
				vertList = #()
				normList = #()
				uvList = #()
				faceList = #()
				matList = #()
				weightList = #()
				indexList = #()
				
				--fill faces first so we have the range of vert indices to use
				if mrge then (
					for sub in pInfo[6] do (
						for f = 1 to sub[3] do ( append matList sub[1] )
						appendIfUnique usedShaders sub[1]
					)
					faceList = pInfo[5]
				)
				else (
					for i = 0 to (sInfo[3] - 1) do (
						append matList sInfo[1]
						append faceList pInfo[5][sInfo[2] + i]
					)
					appendIfUnique usedShaders sInfo[1]
				)
				
				if mrge then (
					for v in pInfo[4] do (
						append vertList v[1]
						append normList v[2]
						append uvList v[3]
						if nIndex == 255 then (
							append indexList v[4]
							append weightList v[5]
						)
						else (
							append indexList #(nIndex + 1)
							append weightList #(1)
						)
					)
				)
				else ( --fill the vertList only with verts in the range of the face indices, and shift the indices to start at 1
					vMin = pInfo[4].count
					vMax = 1
					for i = 1 to faceList.count do (
						if faceList[i].x < vMin do ( vMin = faceList[i].x )
						if faceList[i].y < vMin do ( vMin = faceList[i].y )
						if faceList[i].z < vMin do ( vMin = faceList[i].z )
						if faceList[i].x > vMax do ( vMax = faceList[i].x )
						if faceList[i].y > vMax do ( vMax = faceList[i].y )
						if faceList[i].z > vMax do ( vMax = faceList[i].z )
					)
					for i = vMin to vMax do (
						append vertList pInfo[4][i][1]
						append normList pInfo[4][i][2]
						append uvList pInfo[4][i][3]
						if nIndex == 255 then (
							append indexList pInfo[4][i][4]
							append weightList pInfo[4][i][5]
						)
						else (
							append indexList #(nIndex + 1)
							append weightList #(1)
						)
					)
					for i = 1 to faceList.count do (
						faceList[i] -= (vMin - 1)
					)
				)
				
				if uv then (
					newMesh = mesh vertices:vertList tVerts:uvList faces:faceList materialIDs:matList
					
					buildTVFaces newMesh
					for i = 1 to newMesh.numfaces do (
						setTVFace newMesh i (getFace newMesh i)
					)
				)
				else (
					newMesh = mesh vertices:vertList faces:faceList
				)
				
				if mrge then (
					newMesh.name = rName + ":" + pName
				)
				else (
					newMesh.name = rName + ":" + pName + ":" + (s as string)
				)
				
				newMesh.Faces[rName] = newMesh.Faces --face group for CE region exports
				
				if norm do (
					max modify mode
					normMod = Edit_Normals ()
					select newMesh
					addModifier newMesh normMod
					normMod.selectBy = 1
					normMod.SelLevel  = #Vertex
					normMod.Unify selection:#{1..normMod.GetNumNormals()}
					normMod.MakeExplicit selection:#{1..normMod.GetNumNormals()}
					
					for i = 1 to newMesh.numverts do (
						sel = #{i}
						norms = #{}
						normMod.ConvertVertexSelection &sel &norms
						
						for j in norms do (
							normMod.SetNormal j normList[i]
						)
					)
					
					--dont want annoying modifier with lines everywhere
					collapseStack newMesh
				)
				
				if version >= 0.1 do (
					if not bit.isNAN pInfo[9] do (
						mult = pInfo[9]
						
						newMesh.transform = (matrix3 [mult,0,0] [0,mult,0] [0,0,mult] [0,0,0])
						newMesh.transform *= pInfo[10]
						newMesh.transform *= (matrix3 [100,0,0] [0,100,0] [0,0,100] [0,0,0])
					)
				)
				
				if unwrap do (
					uvMod = Unwrap_UVW ()
					addModifier newMesh uvMod
				)
				
				if (vFormat > 0 or nIndex != 255) and weight do
				(
					max modify mode
					theSkin = Skin()
					select newMesh
					addModifier newMesh theSkin
					for b in boneList do (
						skinOps.addbone theSkin b 0
					)
					redrawViews()
					for v = 1 to vertList.count do (
						skinOps.ReplaceVertexWeights theSkin v indexList[v] weightList[v]
					)
					
					if vFormat == 2 or nIndex != 255 do ( theSkin.rigid_vertices = true )
				)
				
				append meshList newMesh
				append meshDic #(pInfo[7], newMesh)
			)
		)
	)
	
	completeRedraw()
)

rollout shaderRollout "AMF Shaders" width:392 height:416
(
	GroupBox grp_bitmaps "Bitmaps" pos:[8,8] width:376 height:64
	label lbl_bitmaps "Bitmaps Folder:" pos:[16,32] width:104 height:16
	edittext edt_bitmaps "" pos:[16,48] width:288 height:16
	button btn_browse "Browse" pos:[312,48] width:64 height:16
	
	GroupBox grp_cc "CC Options" pos:[8,80] width:120 height:120 enabled:false
	checkbox chk_cc "Use CC Maps" pos:[16,102] width:104 height:16 enabled:false checked:true
	checkbox chk_ccGlobal "Global Colours" pos:[16,120] width:104 height:16 enabled:false checked:true
	label lbl_prim "Primary:" pos:[16,138] width:64 height:16 enabled:false
	label lbl_sec "Secondary:" pos:[16,156] width:64 height:16 enabled:false
	label lbl_tert "Tertiary:" pos:[16,174] width:64 height:16 enabled:false
	colorPicker cp_prim "" pos:[96,139] width:25 height:18 enabled:false color:(color 255 255 255)
	colorPicker cp_sec "" pos:[96,157] width:25 height:18 enabled:false color:(color 255 255 255)
	colorPicker cp_tert "" pos:[96,175] width:25 height:18 enabled:false color:(color 0 0 255)
	
	GroupBox grp_bump "Bump Options" pos:[136,80] width:120 height:120 enabled:false
	checkbox chk_bump "Use Bump Maps" pos:[144,102] width:104 height:16 enabled:false checked:true
	label lbl_bumpAmount "Amount:" pos:[144,120] width:56 height:16 enabled:false
	label lbl_bumpMult "Multiplier:" pos:[144,138] width:56 height:16 enabled:false
	spinner spn_bump1 "" pos:[208,120] width:40 height:16 enabled:false range:[0,100,100] type:#integer
	spinner spn_bump2 "" pos:[208,138] width:40 height:16 enabled:false range:[0,10,1.0]
	
	GroupBox grp_spec "Specular Options" pos:[264,80] width:120 height:120
	checkbox chk_spec "Use Specular" pos:[272,102] width:104 height:16 enabled:false checked:true
	checkbox chk_diffspec "Use Diffuse Spec" pos:[272,120] width:104 height:16 checked:true
	label lbl_specAmount "Amount:" pos:[272,138] width:56 height:16
	spinner spn_spec "" pos:[336,138] width:40 height:16 range:[0,100,50] type:#integer
	checkbox chk_specTint "Specular Tint" pos:[272,156] width:104 height:16 enabled:false checked:true
	label lbl_specTintAmt "Amount:" pos:[272,174] width:56 height:16 enabled:false
	spinner spn_specTint "" pos:[336,174] width:40 height:16 enabled:false range:[0,100,50] type:#integer
	
	GroupBox grp_detail "Detail Options" pos:[136,208] width:120 height:120 enabled:false
	checkbox chk_detail "Use Diff Detail" pos:[144,230] width:104 height:16 enabled:false checked:true
	label lbl_dOpacity "Opacity:" pos:[144,248] width:56 height:16 enabled:false
	spinner spn_dOpacity "" pos:[208,248] width:40 height:16 enabled:false range:[0,100,100] type:#integer
	checkbox chk_dBump "Use Bump Detail" pos:[144,266] width:104 height:16 enabled:false checked:true
	label lbl_dBumpMult "Multiplier:" pos:[144,284] width:56 height:16 enabled:false
	spinner spn_dBump "" pos:[208,284] width:40 height:16 enabled:false range:[0,20,5]
	
	GroupBox grp_illum "Illum Options" pos:[8,208] width:120 height:120 enabled:false
	checkbox chk_illum "Use Illum Maps" pos:[16,230] width:104 height:16 enabled:false checked:true
	label lbl_illumAmount "Amount:" pos:[16,248] width:56 height:16 enabled:false
	spinner spn_illum "" pos:[80,248] width:40 height:16 enabled:false range:[0,100,100] type:#integer
	checkbox chk_illumTint "Illumination Tint" pos:[16,266] width:104 height:16 enabled:false checked:true
	label lbl_illumTintAmt "Amount:" pos:[16,284] width:56 height:16 enabled:false
	spinner spn_illumTint "" pos:[80,284] width:40 height:16 enabled:false range:[0,100,100] type:#integer
	
	GroupBox grp_other "Other Options" pos:[264,208] width:120 height:120
	label lbl_fExt "Extension:" pos:[272,230] width:56 height:16
	edittext edt_fExt "" pos:[333,230] width:43 height:16
	label lbl_matID "Material ID:" pos:[272,248] width:56 height:16
	spinner spn_matID "" pos:[336,248] width:40 height:16 range:[1,30,1] type:#integer
	checkbox chk_sel "Selective Import" pos:[272,266] width:104 height:16 checked:true
	checkbox chk_opac "Use Opacity Maps" pos:[272,284] width:104 height:16 enabled:false checked:true
	label lbl_albAmount "Alb Tint:" pos:[272,302] width:56 height:16 enabled:false
	spinner spn_albAmount "" pos:[336,302] width:40 height:16 enabled:false range:[0,100,100] type:#integer
	
	GroupBox grp_apply "Apply Shaders" pos:[8,336] width:376 height:72
	button btn_apply "Apply" pos:[80,360] width:104 height:32
	button btn_cancel "Close" pos:[208,360] width:104 height:32
	
	on shaderRollout open do (
		edt_fExt.text = ".tif"
		
-- 	[0]	shaderInfo #(sName, paths, uvTiles, isTrans, ccOnly, tints)
-- 	[1]	shaderInfo #(sName, blendPath, blendTile, baseMaps, baseTiles, bumpMaps, bumpTiles, detMaps, detTiles)
		
		for i = 1 to shaderInfo.count do (
			sh = shaderInfo[i]
			
			if shaderTypes[i] == 1 do ( --terrain blend
				if sh[6].count > 0 do ( --bumps
					grp_bump.enabled = chk_bump.enabled = true
					lbl_bumpAmount.enabled = lbl_bumpMult.enabled = true
					spn_bump1.enabled = spn_bump2.enabled = true
				)
				if sh[8].count > 0 do ( --details
					grp_detail.enabled = chk_detail.enabled = true
					lbl_dOpacity.enabled = spn_dOpacity.enabled = true
				)
				continue
			)
			
			if sh[2][1] != "null" do ( --diffuse
				
			)
			if sh[2][2] != "null" do ( --diffuse detail
				grp_detail.enabled = chk_detail.enabled = true
				lbl_dOpacity.enabled = spn_dOpacity.enabled = true
			)
			if sh[2][3] != "null" do ( --colour change
				grp_cc.enabled = true
				chk_cc.enabled = chk_ccGlobal.enabled = true
				lbl_prim.enabled = lbl_sec.enabled = lbl_tert.enabled = true
				cp_prim.enabled = cp_sec.enabled = cp_tert.enabled = true
			)
			if sh[2][4] != "null" do ( --bump
				grp_bump.enabled = chk_bump.enabled = true
				lbl_bumpAmount.enabled = lbl_bumpMult.enabled = true
				spn_bump1.enabled = spn_bump2.enabled = true
			)
			if sh[2][5] != "null" and sh[2][4] != "null" do ( --bump detail
				grp_detail.enabled = chk_dBump.enabled = true
				lbl_dBumpMult.enabled = spn_dBump.enabled = true
			)
			if sh[2][6] != "null" do ( --self illumination
				grp_illum.enabled = chk_illum.enabled = true
				lbl_illumAmount.enabled = spn_illum.enabled = true
				
				if sh[6][2][4] != 0 do ( --illum tint
					chk_illumTint.enabled = lbl_illumTintAmt.enabled = spn_illumTint.enabled = true
				)
			)
			if sh[2][7] != "null" do ( --specular
				chk_spec.enabled = true
			)
			if sh[2][8] != "null" do ( --unused
				
			)
			if sh[4] do ( --opacity flag
				chk_opac.enabled = true
			)
			
			if sh[6][1][4] != 0 do ( --albedo tint
				lbl_albAmount.enabled = spn_albAmount.enabled = true
			)
			--illum above
			if sh[6][3][4] != 0 do ( --spec tint
				chk_specTint.enabled = lbl_specTintAmt.enabled = spn_specTint.enabled = true
			)
			if sh[6][4][4] != 0 do ( --unused
				
			)
		)
	)
	on btn_browse pressed do (
		bPath = getSavePath caption:"Select your root data folder"
		if bPath != undefined do ( edt_bitmaps.text = bPath )
	)
	on chk_illumTint changed val do (
		lbl_illumTintAmt.enabled = spn_illumTint.enabled = val
	)
	on chk_specTint changed val do (
		lbl_specTintAmt.enabled = spn_specTint.enabled = val
	)
	on chk_cc changed val do (
		chk_ccGlobal.enabled = val
		lbl_prim.enabled = lbl_sec.enabled = lbl_tert.enabled = val
		cp_prim.enabled = cp_sec.enabled = cp_tert.enabled = val
	)
	on chk_bump changed val do (
		lbl_bumpAmount.enabled = lbl_bumpMult.enabled = val
		spn_bump1.enabled = spn_bump2.enabled = val
		
		if val then (
			chk_dBump.enabled = val
			lbl_dBumpMult.enabled = spn_dBump.enabled =chk_dBump.checked
		)
		else (
			chk_dBump.enabled = lbl_dBumpMult.enabled = spn_dBump.enabled = val
		)
	)
	on chk_spec changed val do (
		if chk_diffspec.checked != true do (
			lbl_specAmount.enabled = spn_spec.enabled = val
		)
	)
	on chk_diffspec changed val do (
		if chk_spec.checked != true or chk_spec.enabled != true do (
			lbl_specAmount.enabled = spn_spec.enabled = val
		)
	)
	on chk_detail changed val do (
		lbl_dOpacity.enabled = spn_dOpacity.enabled = val
	)
	on chk_dBump changed val do (
		lbl_dBumpMult.enabled = spn_dBump.enabled = val
	)
	on chk_illum changed val do (
		lbl_illumAmount.enabled = spn_illum.enabled = val
		if val then (
			chk_illumTint.enabled = val
			lbl_illumTintAmt.enabled = spn_illumTint.enabled = chk_illumTint.checked
		)
		else (
			chk_illumTint.enabled = lbl_illumTintAmt.enabled = spn_illumTint.enabled = val
		)
	)
	on btn_apply pressed do (
		if edt_bitmaps.text == "" do ( return() )
		
		bitmPath = edt_bitmaps.text + "\\"
		
-- 	[0]	shaderInfo #(sName, paths, uvTiles, isTrans, ccOnly, tints)
-- 	[1]	shaderInfo #(sName, blendPath, blendTile, baseMaps, baseTiles, bumpMaps, bumpTiles, detMaps, detTiles)
		
		shaderNames = #()
		matList = #()
		for i = 1 to shaderInfo.count do (
			append shaderNames shaderInfo[i][1]
			append matList i 
		)
			
		mat = multimaterial numsubs:shaderInfo.count names:shaderNames materialIDList:matList
		
		primary_colour = Color_Correction color:cp_prim.color
		secondary_colour = Color_Correction color:cp_sec.color
		tertiary_colour = Color_Correction color:cp_tert.color
		
		for mc = 1 to mat.materialList.count do (
			if chk_sel.checked do (
				found = false
				for id in usedShaders do (
					if id == mc do ( found = true )
				)
				
				if not found do (
					mat.names[mc] = "unused"
					mat.materialList[mc].name = "unused"
					continue
				)
			)
			
			cShader = shaderInfo[mc]
			
			mat.materialList[mc].name = cShader[1]
			mat.materialList[mc].showInViewport = true
			fileNames = #()
			
			if shaderTypes[mc] == 1 do (
				--shaderInfo #(sName, blendPath, blendTile, baseMaps, baseTiles, bumpMaps, bumpTiles, detMaps, detTiles)
				
				if cShader[2] != "null" do (
					fName = bitmPath + cShader[2] + edt_fExt.text
					
					bTex = bitmaptexture filename:fName
					rewire = 9
					if cShader[4].count == 4 do ( rewire = 7 )
					
					b0 = Color_Correction map:bTex rewireR:10 rewireG:10 rewireB:10 rewireA:7
					b1 = Color_Correction map:bTex rewireR:0 rewireG:10 rewireB:10 rewireA:rewire
					b2 = Color_Correction map:bTex rewireR:10 rewireG:1 rewireB:10 rewireA:9
					b3 = Color_Correction map:bTex rewireR:10 rewireG:10 rewireB:2 rewireA:9
					
					comp0 = CompositeTextureMap()
					comp0.mapList[1] = b0
					comp0.mapList[2] = RGB_Tint()
					comp0.blendMode[2] = 23
					
					comp1 = CompositeTextureMap()
					comp1.mapList[1] = b1
					comp1.mapList[2] = RGB_Tint()
					comp1.blendMode[2] = 23
					
					comp2 = CompositeTextureMap()
					comp2.mapList[1] = b2
					comp2.mapList[2] = RGB_Tint()
					comp2.blendMode[2] = 23
					
					comp3 = CompositeTextureMap()
					comp3.mapList[1] = b3
					comp3.mapList[2] = RGB_Tint()
					comp3.blendMode[2] = 23
				)
				
				if cShader[4].count > 0 do (
					comp = CompositeTextureMap()
					baseComp = CompositeTextureMap()
					
					comp.mapList[1] = baseComp
					comp.layerName[1] = "Diffuse - "
					comp.blendMode[1] = 14
					
					for x = 1 to cShader[4].count do (
						baseComp.blendMode[x] = 14
					)
					
					if cShader[2] != "null" do (
						baseComp.mask[1] = comp1
						
						if cShader[4].count > 1 do (
							baseComp.mask[2] = comp2
						)
						
						if cShader[4].count > 2 do (
							baseComp.mask[3] = comp3
						)
						
						if cShader[4].count > 3 do (
							baseComp.mask[1] = comp0
							baseComp.mask[2] = comp2
							baseComp.mask[3] = comp3
							baseComp.mask[4] = comp1
						)
					)
					
					for x = 1 to cShader[4].count do (
						baseComp.mapList[x] = bitmaptexture filename:(bitmPath + cShader[4][x] + edt_fExt.text) alphasource:2
						baseComp.mapList[x].coords.u_tiling = cShader[5][x].x
						baseComp.mapList[x].coords.v_tiling = cShader[5][x].y
					)
					
					if cShader[8].count > 0 and chk_detail.checked do (
						detComp = CompositeTextureMap()
						
						comp.mapList[2] = detComp
						comp.layerName[2] = "Detail - "
						comp.blendMode[2] = 14
						
						for x = 1 to cShader[8].count do (
							detComp.blendMode[x] = 14
						)
						
						if cShader[2] != "null" do (
							detComp.mask[1] = comp1
							
							if cShader[8].count > 1 do (
								detComp.mask[2] = comp2
							)
							
							if cShader[8].count > 2 do (
								detComp.mask[3] = comp3
							)
							
							if cShader[8].count > 3 do (
								detComp.mask[1] = comp0
								detComp.mask[2] = comp2
								detComp.mask[3] = comp3
								detComp.mask[4] = comp1
							)
						)
						
						for x = 1 to cShader[8].count do (
							detComp.mapList[x] = bitmaptexture filename:(bitmPath + cShader[8][x] + edt_fExt.text) alphasource:2
							detComp.mapList[x].coords.u_tiling = cShader[9][x].x
							detComp.mapList[x].coords.v_tiling = cShader[9][x].y
						)
					)
					
					mat.materialList[mc].diffuseMap = comp
				)
				
				if cShader[6].count > 0 and chk_bump.checked do (
					bumpComp = CompositeTextureMap()
					bumpMap = Normal_Bump normal_map:bumpComp
					
					for x = 1 to cShader[6].count do (
						if x > 3 do ( continue ) --4th bump is just footsteps
						bumpComp.blendMode[x] = 14
					)
					
					if cShader[2] != "null" do (
						bumpComp.mask[1] = comp1
						
						if cShader[6].count > 1 do (
							bumpComp.mask[2] = comp2
						)
						
						if cShader[6].count > 2 do (
							bumpComp.mask[3] = comp3
						)
					)
					
					for x = 1 to cShader[6].count do (
						if x > 3 do ( continue )
						bumpComp.mapList[x] = bitmaptexture filename:(bitmPath + cShader[6][x] + edt_fExt.text) alphasource:2
						bumpComp.mapList[x].coords.u_tiling = cShader[7][x].x
						bumpComp.mapList[x].coords.v_tiling = cShader[7][x].y
					)
					
					bumpMap.mult_spin = spn_bump2.value
					mat.materialList[mc].bumpMap = bumpMap
					mat.materialList[mc].bumpMapAmount = spn_bump1.value
				)
				
				continue
			)
			
			for fc = 1 to 8 do (
				append fileNames (bitmPath + cShader[2][fc] + edt_fExt.text)
			)
			
			/*
			1: diffuse
			2: detail
			3: CC
			4: bump
			5: detail bump
			6: illum
			7: specular (diffuse alpha is used if specular is null)
			8: reflection (unused, not exported)
			*/
			
			--Diffuse, Detail and CC
			if cShader[2][1] != "null" or cShader[2][3] != "null" do (
				comp = CompositeTextureMap()
				num = 0 --index offset if detail present
				
				if not cShader[5] do ( -- if not CC only
					texture = bitmaptexture filename:fileNames[1] alphasource:2
					texture.coords.u_tiling = cShader[3][1].x
					texture.coords.v_tiling = cShader[3][1].y
					
					if cShader[2][2] != "null" and chk_detail.checked do ( -- if detail map exists
						num = 1
						detail =  bitmaptexture filename:fileNames[2] alphasource:2
						detail.coords.u_tiling = cShader[3][2].x
						detail.coords.v_tiling = cShader[3][2].y
						
						comp.mapList[1] = detail
						comp.layerName[1] = "Detail Map - "
						comp.blendMode[1] = 14
						comp.opacity[1] = spn_dOpacity.value
					)
					
					comp.mapList[num + 1] = texture
					comp.layerName[num + 1] = "Diffuse Map - "
					comp.blendMode[num + 1] = 14
					
					if cShader[6][1][4] != 0 and spn_albAmount.value > 0 do ( -- if tint isnt invisible and alb is on
						num += 1
						comp.mapList[num + 1] = Color_Correction color:(color cShader[6][1][1] cShader[6][1][2] cShader[6][1][3] cShader[6][1][4])
						comp.layerName[num + 1] = "Albedo Tint - "
						comp.blendMode[num + 1] = 5
						comp.opacity[num + 1] = spn_albAmount.value
					)
				)
				
				if cShader[2][3] != "null" and (maxVersion())[1] >= 10000 and chk_cc.checked then (
					ccTex = bitmaptexture filename:fileNames[3]
					CC1 = Color_Correction map:ccTex rewireR:0 rewireG:10 rewireB:10 rewireA:9
					CC2 = Color_Correction map:ccTex rewireR:10 rewireG:1 rewireB:10 rewireA:9
					CC3 = Color_Correction map:ccTex rewireR:10 rewireG:10 rewireB:2 rewireA:9
					
					comp1 = CompositeTextureMap()
					comp1.mapList[1] = CC1
					comp1.mapList[2] = RGB_Tint()
					comp1.blendMode[2] = 23
					
					comp2 = CompositeTextureMap()
					comp2.mapList[1] = CC2
					comp2.mapList[2] = RGB_Tint()
					comp2.blendMode[2] = 23
					
					comp3 = CompositeTextureMap()
					comp3.mapList[1] = CC3
					comp3.mapList[2] = RGB_Tint()
					comp3.blendMode[2] = 23
					
					if chk_ccGlobal.checked then (
						comp.mapList[num + 2] = primary_colour
						comp.mapList[num + 3] = secondary_colour
						comp.mapList[num + 4] = tertiary_colour
					)
					else (
						comp.mapList[num + 2] = Color_Correction color:cp_prim.color
						comp.mapList[num + 3] = Color_Correction color:cp_sec.color
						comp.mapList[num + 4] = Color_Correction color:cp_tert.color
					)
					comp.mask[num + 2] = comp1
					comp.mask[num + 3] = comp2
					comp.mask[num + 4] = comp3
					comp.blendMode[num + 2] = 5
					comp.blendMode[num + 3] = 5
					comp.blendMode[num + 4] = 5
					comp.layerName[num + 2] = "Primary Colour - "
					comp.layerName[num + 3] = "Secondary Colour - "
					comp.layerName[num + 4] = "Tertiary Colour - "
					
				)
				mat.materialList[mc].diffuseMap = comp
			)
			
			--Bump and Detail Bump
			if cShader[2][4] != "null" and chk_bump.checked do (
				texture = bitmaptexture filename:fileNames[4] alphasource:2
				texture.coords.u_tiling = cShader[3][4].x
				texture.coords.v_tiling = cShader[3][4].y
				bumpMap = Normal_Bump normal_map:texture
				bumpMap.mult_spin = spn_bump2.value
				if cShader[2][5] != "null" and chk_dBump.checked do (
					texture2 = bitmaptexture filename:fileNames[5] alphasource:2
					texture2.coords.u_tiling = cShader[3][5].x
					texture2.coords.v_tiling = cShader[3][5].y
					bumpMap.bump_map = texture2
					bumpMap.bump_spin = spn_dBump.value
				)
				mat.materialList[mc].bumpMap = bumpMap
				mat.materialList[mc].bumpMapAmount = spn_bump1.value
			)
			
			--Illumination
			if cShader[2][6] != "null" and chk_illum.checked do (
				texture = bitmaptexture filename:fileNames[6] alphasource:2
				texture.coords.u_tiling = cShader[3][6].x
				texture.coords.v_tiling = cShader[3][6].y
				
				if chk_illumTint.checked then (
					comp = CompositeTextureMap()
					
					comp.mapList[1] = texture
					comp.layerName[1] = "Illumination Map - "
					
					comp.mapList[2] = Color_Correction color:(color cShader[6][2][1] cShader[6][2][2] cShader[6][2][3] cShader[6][2][4])
					comp.layerName[2] = "Illumination Tint - "
					comp.blendMode[2] = 5
					comp.opacity[2] = spn_illumTint.value
					
					mat.materialList[mc].selfIllumMap = comp
				)
				else (
					mat.materialList[mc].selfIllumMap = texture
				)
					
				mat.materialList[mc].selfIllumMapAmount = spn_illum.value
				mat.materialList[mc].useSelfIllumColor = true
			)
			
			--Specular
			if cShader[2][7] != "null" and chk_spec.checked then (
				texture = bitmaptexture filename:fileNames[7] alphasource:0 monoOutput:1 rgbOutput:1
				texture.coords.u_tiling = cShader[3][7].x
				texture.coords.v_tiling = cShader[3][7].y
				mat.materialList[mc].specularLevelMap = texture
				mat.materialList[mc].specularLevelMapAmount = spn_spec.value
			)
			else ( --Specular from Diffuse alpha
				if ((cShader[2][1] != "null" and chk_diffspec.checked) or cShader[5]) and not cShader[4] then (
					comp = CompositeTextureMap()
					num = 0
					texture = bitmaptexture filename:fileNames[1] alphasource:0 monoOutput:1 rgbOutput:1
					if cShader[5] do (
						texture = bitmaptexture filename:fileNames[3] alphasource:0 monoOutput:1 rgbOutput:1
					)
					texture.coords.u_tiling = cShader[3][1].x
					texture.coords.v_tiling = cShader[3][1].y
					
					if cShader[2][2] != "null" and chk_detail.checked do (
						num = 1
						detail = bitmaptexture filename:fileNames[2] alphasource:0 monoOutput:1 rgbOutput:1
						detail.coords.u_tiling = cShader[3][2].x
						detail.coords.v_tiling = cShader[3][2].y
						
						comp.mapList[1] = detail
						comp.layerName[1] = "Detail Spec - "
						comp.blendMode[1] = 14
						comp.opacity[1] = spn_dOpacity.value
					)
					
					comp.mapList[num + 1] = texture
					comp.layerName[num + 1] = "Diffuse Spec - "
					comp.blendMode[num + 1] = 14
					
					mat.materialList[mc].specularLevelMap = comp
					mat.materialList[mc].specularLevelMapAmount = spn_spec.value
				)
			)
			
			if cShader[6][3][4] != 0 and chk_specTint.checked do ( -- if tint isnt invisible
				mat.materialList[mc].specularMap = Color_Correction color:(color cShader[6][3][1] cShader[6][3][2] cShader[6][3][3] cShader[6][3][4])
				mat.materialList[mc].specularMapAmount = spn_specTint.value
			)
			
			if cShader[4] do (
				if cShader[5] then (
					mat.materialList[mc].opacityMap = bitmaptexture filename:fileNames[3] alphasource:0 monoOutput:1 rgbOutput:1
				)
				else (
					if cShader[2][1] != "null" and chk_opac.checked do (
						mat.materialList[mc].opacityMap = bitmaptexture filename:fileNames[1] monoOutput:1 alphasource:0
					)
				)
			)
		)
		
		meditMaterials[spn_matID.value] = mat
		meditMaterials[spn_matID.value].name = mName
		
		for count = 1 to meshList.count do (
			meshList[count].material = meditMaterials[spn_matID.value]
		)
		
		completeRedraw()
	)
	on btn_cancel pressed do (
		destroyDialog shaderRollout
	)
)

rollout mainRollout "AMF Importer v2.1.1" width:368 height:488
(
	GroupBox grp_load "Load File" pos:[8,8] width:352 height:72
	button btn_load "Load Model File" pos:[120,32] width:122 height:29
	
	label lbl_selMesh "Select Meshes:" pos:[8,88] width:96 height:16 enabled:false
	dotNetControl tvRegions "System.Windows.Forms.TreeView" pos:[8,104] width:184 height:296
	
	GroupBox grp_nodemarker "Node/Marker Options" pos:[200,96] width:160 height:114 enabled:false
	checkbox chk_nodes "Import Nodes" pos:[208,114] width:144 height:16 enabled:false checked:true
	checkbox chk_markers "Import Markers" pos:[208,132] width:144 height:16 enabled:false checked:true
	label lbl_nRadius "Nodes Radius:" pos:[208,150] width:70 height:16 enabled:false
	label lbl_mRadius "Marker Radius:" pos:[208,168] width:72 height:16 enabled:false
	spinner spn_nRadius "" pos:[288,150] width:40 height:16 enabled:false range:[0.1,250,2]
	spinner spn_mRadius "" pos:[288,168] width:40 height:16 enabled:false range:[0.1,250,1]
	checkbox chk_bones "SetBoneEnable" pos:[208,186] width:144 height:16 enabled:false checked:true
	
	GroupBox grp_mesh "Mesh Options" pos:[200,218] width:160 height:94 enabled:false
	checkbox chk_mesh "Import Meshes" pos:[208,237] width:144 height:16 enabled:false checked:true
	checkbox chk_merge "Merge Submeshes" pos:[208,255] width:144 height:16 enabled:false checked:true
	checkbox chk_norm "Import Normals" pos:[208,273] width:144 height:16 enabled:false checked:false
	checkbox chk_weight "Weight Meshes" pos:[208,291] width:144 height:16 enabled:false checked:true
	
	GroupBox grp_material "Material Options" pos:[200,320] width:160 height:80 enabled:false
	checkbox chk_uv "Import UVW and MatIDs" pos:[208,338] width:144 height:16 enabled:false checked:true
	checkbox chk_unwrap "Unwrap UVW" pos:[208,356] width:144 height:16 enabled:false
	checkbox chk_shader "Setup Shader Library" pos:[208,374] width:144 height:16 enabled:false checked:true
	
	GroupBox grp_import "Import" pos:[8,408] width:352 height:72 enabled:false
	button btn_import "Import Model" pos:[112,432] width:144 height:32 enabled:false
	
	on mainRollout open do
	(
		tvRegions.checkboxes = true
		tvRegions.enabled = false
	)
	on btn_load pressed do
	(
		in_name = getOpenFileName caption:"Select AMF model file" types:"Adjutant Model Files (*.amf) | *.amf|"()
		
		if in_name != undefined then (
			inFile = fOpen in_name "rb"
			
			readHeader()
			
			if header != 558255425 do (
				messageBox "Invalid file." title:"Error"
				FClose inFile
				return()
			)
			
			if version > 2.1 do (
				messageBox "Unknown format version. Try updating your AMF script." title:"Error"
				FClose inFile
				return()
			)
			
			tvRegions.Nodes.Clear()
			
			readNodes()
			readMarkers()
			readRegions()	
			readShaders()
			loadRegions(tvRegions)
			
			FClose inFile
			
			if loaded == false do (
				lbl_selMesh.enabled = tvRegions.enabled = true
				
				grp_nodemarker.enabled = true
				chk_nodes.enabled = lbl_nRadius.enabled = spn_nRadius.enabled = true
				chk_markers.enabled = lbl_mRadius.enabled = spn_mRadius.enabled = true
				chk_bones.enabled = true
				
				grp_mesh.enabled = true
				chk_mesh.enabled = chk_merge.enabled = chk_norm.enabled = chk_weight.enabled = true
				
				grp_material.enabled = true
				chk_uv.enabled = chk_unwrap.enabled =  chk_shader.enabled = true
				
				grp_import .enabled = btn_import.enabled = true
				
				loaded = true
			)
		)
	)
	on chk_nodes changed val do
	(
		lbl_nRadius.enabled = spn_nRadius.enabled = chk_bones.enabled = val
		
		if val == false then (
			chk_markers.enabled = lbl_mRadius.enabled = spn_mRadius.enabled = val
		)
		else (
			chk_markers.enabled = val
			lbl_mRadius.enabled = spn_mRadius.enabled = chk_markers.checked
		)
		
		if chk_mesh.checked and val == false do (
			chk_weight.enabled = val
		)
		
		if chk_mesh.checked and chk_weight.enabled == false and val do (
			chk_weight.enabled = val
		)
	)
	on chk_markers changed val do
	(
		lbl_mRadius.enabled = spn_mRadius.enabled = val
	)
	on spn_nRadius changed val do
	( 
		nodeRadius = val 
	)
	on spn_mRadius changed val do
	( 
		markRadius = val 
	)
	on chk_mesh changed val do
	(
		lbl_selMesh.enabled = tvRegions.enabled = val
		
		chk_merge.enabled = chk_norm.enabled = val
		
		if chk_nodes.checked do (
			chk_weight.enabled = val
		)
		
		grp_material.enabled = val
		chk_uv.enabled = val
		
		if val then (
			chk_unwrap.enabled =  chk_shader.enabled = chk_uv.checked
		)
		else (
			chk_unwrap.enabled =  chk_shader.enabled = val
		)
	)
	on chk_uv changed val do
	(
		chk_unwrap.enabled = chk_shader.enabled = val
	)
	on btn_import pressed do
	(
		if chk_nodes.checked do 
		( 
			createBones spn_nRadius.value chk_bones.checked
			if chk_markers.checked do ( createMarkers spn_mRadius.value )
		)
		if chk_mesh.checked do (
			createMeshes chk_merge.checked chk_norm.checked (chk_weight.checked and chk_nodes.checked) chk_uv.checked chk_unwrap.checked tvRegions 
		)
		
		--revert bone names
		if chk_nodes.checked do (
			for b in boneList do (
				bName = b.name as string
				b.name = (substring bName 4 (bName.count - 3))
			)
		)
		
		clearSelection()
		completeRedraw()
		
		if chk_shader.enabled and chk_shader.checked do ( createDialog shaderRollout )
	)
)

createDialog mainRollout