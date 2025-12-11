<template>
  <div ref="sceneRef" class="scene-background"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import * as THREE from 'three'
import { FontLoader } from 'three/examples/jsm/loaders/FontLoader.js'
import { TextGeometry } from 'three/examples/jsm/geometries/TextGeometry.js'
import { EffectComposer } from 'three/examples/jsm/postprocessing/EffectComposer.js'
import { RenderPass } from 'three/examples/jsm/postprocessing/RenderPass.js'
import { UnrealBloomPass } from 'three/examples/jsm/postprocessing/UnrealBloomPass.js'
import { OutputPass } from 'three/examples/jsm/postprocessing/OutputPass.js'

const sceneRef = ref<HTMLElement | null>(null)

let renderer: THREE.WebGLRenderer
let scene: THREE.Scene
let camera: THREE.PerspectiveCamera
let composer: EffectComposer
let bloomPass: UnrealBloomPass
let animationId: number
let clock: THREE.Clock

let sphere: THREE.Mesh
let rings: THREE.Group
let particles: THREE.Group
let particles2: THREE.Group
let textGroup: THREE.Group
let textMeshes: THREE.Mesh[] = []
let orbitingObjects: any[] = []

const COLORS = {
  primary: '#a78bfa',
  secondary: '#7c3aed',
  cyan: '#26C6DA',
  background: '#0f172a',

  //Dla gwiazdek
  white: '#a1a1aa',
  pink: '#ec4899',

  //Pionki
  pawnNavy: '#2563eb',
  pawnRed: '#dc2626',
  pawnGreen: '#059669',
  pawnYellow: '#d97706',
}

const fontPath: string = '/fonts/Nasalization Rg_Regular.json'

const init = () => {
  if (!sceneRef.value) return

  const width = sceneRef.value?.clientWidth
  const height = sceneRef.value?.clientHeight

  clock = new THREE.Clock()

  scene = new THREE.Scene()
  scene.background = new THREE.Color(COLORS.background)

  camera = new THREE.PerspectiveCamera(60, width / height, 0.1, 1000)
  camera.position.set(0, 1, 8)
  camera.lookAt(0, 0, 0)

  renderer = new THREE.WebGLRenderer({ antialias: true })
  renderer.setSize(width, height)
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
  renderer.toneMapping = THREE.ReinhardToneMapping
  renderer.toneMappingExposure = 1

  sceneRef.value.appendChild(renderer.domElement)

  setupPostProcessing(width, height)

  createSphere()
  createRings()
  createText()
  createParticles()
  createOrbitingObjects()
}

const setupPostProcessing = (width: number, height: number) => {
  composer = new EffectComposer(renderer)

  const renderPass = new RenderPass(scene, camera)
  composer.addPass(renderPass)

  bloomPass = new UnrealBloomPass(new THREE.Vector2(width, height), 0.9, 0.3, 0.1)
  composer.addPass(bloomPass)

  const outputPass = new OutputPass()
  composer.addPass(outputPass)
}

const createSphere = () => {
  const spehereGeometry = new THREE.SphereGeometry(1.4, 24, 24)
  const sphereMaterial = new THREE.MeshBasicMaterial({
    color: COLORS.secondary,
    wireframe: true,
  })
  sphere = new THREE.Mesh(spehereGeometry, sphereMaterial)
  sphere.position.y = 2
  scene.add(sphere)
}

const createRings = () => {
  rings = new THREE.Group()

  const ringsConfig = [
    { radius: 2.2, tubeRadius: 0.04, color: COLORS.cyan },
    { radius: 2.5, tubeRadius: 0.05, color: COLORS.secondary },
  ]

  const ringsCount: number = ringsConfig.length

  for (let i = 0; i < ringsCount; i++) {
    const ringGeometry = new THREE.TorusGeometry(
      ringsConfig[i].radius,
      ringsConfig[i].tubeRadius,
      16,
      100,
    )
    const ringMaterial = new THREE.MeshBasicMaterial({ color: ringsConfig[i].color })
    const ring = new THREE.Mesh(ringGeometry, ringMaterial)
    ring.rotation.x = Math.PI / 4
    rings.add(ring)
  }

  rings.rotation.x = Math.PI * 0.35
  rings.rotation.z = -Math.PI / 6
  rings.position.y = 1.5
  scene.add(rings)
}

const createText = () => {
  const loader = new FontLoader()

  const textConfig = [
    { text: 'DIGITAL', position: -1 },
    { text: 'WARS', position: -2.5 },
  ]

  const textMaterial = new THREE.MeshBasicMaterial({ color: COLORS.secondary })

  loader.load(fontPath, (font) => {
    textGroup = new THREE.Group()
    textMeshes = []

    for (let i = 0; i < textConfig.length; i++) {
      const textGeometry = new TextGeometry(textConfig[i].text, {
        font: font,
        size: 0.9,
        depth: 0.2,
        curveSegments: 12,
        bevelEnabled: true,
        bevelThickness: 0.05,
        bevelSize: 0.04,
        bevelOffset: 0,
        bevelSegments: 5,
      })
      textGeometry.computeBoundingBox()
      if (!textGeometry.boundingBox) return

      const centerOffset = -0.5 * (textGeometry.boundingBox.max.x - textGeometry.boundingBox.min.x)
      const textMesh = new THREE.Mesh(textGeometry, textMaterial.clone())
      textMesh.position.x = centerOffset
      textMesh.position.y = textConfig[i].position
      textMesh.position.z = 0

      textMeshes.push(textMesh)
      textGroup.add(textMesh)
    }

    scene.add(textGroup)
  })
}

function createParticles() {
  particles = new THREE.Group()
  particles2 = new THREE.Group()

  const createParticleCloud = (
    count: number,
    color: string,
    size: number,
    spread: { x: number; y: number; z: number },
    zOffset: number,
  ) => {
    const geometry = new THREE.BufferGeometry()
    const positions = new Float32Array(count * 3)

    for (let i = 0; i < count; i++) {
      positions[i * 3] = (Math.random() - 0.5) * spread.x
      positions[i * 3 + 1] = (Math.random() - 0.5) * spread.y
      positions[i * 3 + 2] = (Math.random() - 0.5) * spread.z + zOffset
    }

    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3))

    const material = new THREE.PointsMaterial({
      color,
      size,
      transparent: true,
      opacity: 0.8,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
    })

    return new THREE.Points(geometry, material)
  }

  particles.add(createParticleCloud(150, COLORS.primary, 0.04, { x: 30, y: 15, z: 10 }, -5))
  particles2.add(createParticleCloud(160, COLORS.secondary, 0.03, { x: 30, y: 20, z: 15 }, -8))
  particles.add(createParticleCloud(140, COLORS.cyan, 0.025, { x: 16, y: 12, z: 8 }, -3))
  particles2.add(createParticleCloud(200, COLORS.pink, 0.025, { x: 40, y: 16, z: 10 }, -5))
  particles.add(createParticleCloud(120, COLORS.white, 0.015, { x: 50, y: 40, z: 30 }, -15))

  scene.add(particles)
}

const createPawn = (color: string) => {
  const pawn = new THREE.Group()
  const material = new THREE.MeshBasicMaterial({ color })

  const baseGeometry = new THREE.CylinderGeometry(0.12, 0.15, 0.08, 16)
  const base = new THREE.Mesh(baseGeometry, material)
  base.position.y = 0.04
  pawn.add(base)

  const bodyGeometry = new THREE.CylinderGeometry(0.06, 0.12, 0.2, 16)
  const body = new THREE.Mesh(bodyGeometry, material)
  body.position.y = 0.18
  pawn.add(body)

  const headGeometry = new THREE.SphereGeometry(0.08, 16, 16)
  const head = new THREE.Mesh(headGeometry, material)
  head.position.y = 0.36
  pawn.add(head)

  return pawn
}

function createDice(color: string) {
  const dice = new THREE.Group()

  const cubeGeometry = new THREE.BoxGeometry(0.28, 0.28, 0.28)
  const cubeMaterial = new THREE.MeshBasicMaterial({ color })
  const cube = new THREE.Mesh(cubeGeometry, cubeMaterial)
  dice.add(cube)

  const edgesGeometry = new THREE.EdgesGeometry(cubeGeometry)
  const edgesMaterial = new THREE.LineBasicMaterial({ color: 0x94a3b8 })
  const edges = new THREE.LineSegments(edgesGeometry, edgesMaterial)
  dice.add(edges)

  const dotGeometry = new THREE.CircleGeometry(0.025, 16)
  const dotMaterial = new THREE.MeshBasicMaterial({ color: 0xffffff })

  const d = 0.141
  const s = 0.065

  const faces: { positions: [number, number, number][]; rotation: [number, number, number] }[] = [
    { positions: [[0, 0, d]], rotation: [0, 0, 0] },
    {
      positions: [
        [-s, s, -d],
        [s, -s, -d],
      ],
      rotation: [0, Math.PI, 0],
    },
    {
      positions: [
        [-d, 0, 0],
        [-d, s, s],
        [-d, -s, -s],
      ],
      rotation: [0, -Math.PI / 2, 0],
    },
    {
      positions: [
        [d, s, s],
        [d, s, -s],
        [d, -s, s],
        [d, -s, -s],
      ],
      rotation: [0, Math.PI / 2, 0],
    },
    {
      positions: [
        [-s, d, -s],
        [-s, d, s],
        [s, d, -s],
        [s, d, s],
        [0, d, 0],
      ],
      rotation: [-Math.PI / 2, 0, 0],
    },
    {
      positions: [
        [-s, -d, s],
        [s, -d, s],
        [-s, -d, -s],
        [s, -d, -s],
        [0, -d, s],
        [0, -d, -s],
      ],
      rotation: [Math.PI / 2, 0, 0],
    },
  ]

  faces.forEach((face) => {
    face.positions.forEach((pos) => {
      const dot = new THREE.Mesh(dotGeometry, dotMaterial)
      dot.position.set(...pos)
      dot.rotation.set(...face.rotation)
      dice.add(dot)
    })
  })

  return dice
}

const createOrbitingObjects = () => {
  const objects = [
    {
      type: 'pawn',
      color: COLORS.pawnRed,
      orbitRadius: 2.8,
      orbitSpeed: 0.18,
      orbitTilt: 0.4,
      startAngle: 0,
    },
    {
      type: 'pawn',
      color: COLORS.pawnGreen,
      orbitRadius: 4.5,
      orbitSpeed: -0.12,
      orbitTilt: -0.6,
      startAngle: Math.PI * 0.4,
    },
    {
      type: 'pawn',
      color: COLORS.pawnNavy,
      orbitRadius: 3.4,
      orbitSpeed: 0.22,
      orbitTilt: 0.8,
      startAngle: Math.PI * 1.1,
    },
    {
      type: 'pawn',
      color: COLORS.pawnYellow,
      orbitRadius: 5.0,
      orbitSpeed: -0.08,
      orbitTilt: -0.3,
      startAngle: Math.PI * 1.7,
    },
    {
      type: 'dice',
      color: COLORS.secondary,
      orbitRadius: 3.0,
      orbitSpeed: -0.25,
      orbitTilt: -0.9,
      startAngle: Math.PI * 0.2,
    },
    {
      type: 'dice',
      color: COLORS.secondary,
      orbitRadius: 4.8,
      orbitSpeed: 0.1,
      orbitTilt: 0.5,
      startAngle: Math.PI * 1.4,
    },
  ]

  objects.forEach((config) => {
    let mesh
    if (config.type === 'pawn') {
      mesh = createPawn(config.color)
      mesh.scale.set(0.9, 0.9, 0.9)
    } else if (config.type === 'dice') {
      mesh = createDice(config.color)
    }
    const pivot = new THREE.Group()
    pivot.rotation.x = config.orbitTilt
    pivot.rotation.z = config.orbitTilt * 0.7

    if (!mesh) return

    mesh.position.x = config.orbitRadius
    mesh.position.y = (Math.random() - 0.5) * 1.5

    pivot.add(mesh)
    pivot.position.y = 1.5
    scene.add(pivot)

    orbitingObjects.push({
      pivot,
      mesh,
      orbitSpeed: config.orbitSpeed,
      startAngle: config.startAngle,
      selfRotationSpeed: {
        x: (Math.random() - 0.5) * 2,
        y: (Math.random() - 0.5) * 2,
        z: (Math.random() - 0.5) * 2,
      },
    })
  })
}

const animate = () => {
  animationId = requestAnimationFrame(animate)

  const elapsed = clock.getElapsedTime()

  const levitationSpeed = 0.4
  const levitationRange = 0.2
  const baseY = 2.0
  const baseZ = 0.0
  const levitationZRange = 0.5

  const yOffset = Math.sin(elapsed * levitationSpeed) * levitationRange
  const zOffset = Math.cos(elapsed * levitationSpeed) * levitationZRange

  if (sphere) {
    sphere.rotation.y = elapsed * 0.1
    sphere.position.y = baseY + yOffset
    sphere.position.z = baseZ + zOffset
  }

  if (rings) {
    rings.position.y = baseY + yOffset
    rings.rotation.z = baseZ + zOffset * 0.6
  }

  if (particles) {
    particles.position.y = elapsed * 0.005
  }

  if (particles2) {
    particles.position.z = elapsed * 0.005
  }

  if (orbitingObjects) {
    orbitingObjects.forEach((obj) => {
      obj.pivot.rotation.y = obj.startAngle + elapsed * obj.orbitSpeed
      obj.mesh.rotation.x += obj.selfRotationSpeed.x * 0.006
      obj.mesh.rotation.y += obj.selfRotationSpeed.y * 0.006
      obj.mesh.rotation.z += obj.selfRotationSpeed.z * 0.006
    })
  }

  composer.render()
}

function handleResize() {
  if (!sceneRef.value) return
  const width = sceneRef.value.clientWidth
  const height = sceneRef.value.clientHeight

  camera.aspect = width / height
  camera.updateProjectionMatrix()

  renderer.setSize(width, height)
  composer.setSize(width, height)
}

onMounted(() => {
  init()
  animate()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  cancelAnimationFrame(animationId)
  renderer.dispose()
  composer.dispose()
})
</script>

<style scoped>
.scene-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 0;
}
</style>
