<template>
  <div ref="container" class="canvas-container"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import * as THREE from 'three'
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js'
import { FontLoader } from 'three/addons/loaders/FontLoader.js'
import { TextGeometry } from 'three/addons/geometries/TextGeometry.js'
import { OrbitControls } from 'three/addons/controls/OrbitControls.js'
import { EffectComposer } from 'three/addons/postprocessing/EffectComposer.js'
import { RenderPass } from 'three/addons/postprocessing/RenderPass.js'
import { UnrealBloomPass } from 'three/addons/postprocessing/UnrealBloomPass.js'
import { OutputPass } from 'three/addons/postprocessing/OutputPass.js'

const container = ref<HTMLDivElement | null>(null)

let scene: THREE.Scene
let camera: THREE.PerspectiveCamera
let renderer: THREE.WebGLRenderer
let controls: OrbitControls
let clock: THREE.Clock
let animationId: number
let rings: THREE.Group
let composer: EffectComposer
let robot: THREE.Group
let particles: THREE.Group
let particles2: THREE.Group
let textGroup: THREE.Group
let orbitingObjects: any[] = []
let sphere: THREE.Mesh

let leftEye: THREE.Object3D | null = null
let rightEye: THREE.Object3D | null = null
let leftPupil: THREE.Object3D | null = null
let rightPupil: THREE.Object3D | null = null
let robotHead: THREE.Object3D | null = null

const fontPath = '/fonts/Nasalization Rg_Regular.json'

const COLORS = {
  primary: '#a78bfa',
  secondary: '#7c3aed',
  cyan: '#26C6DA',
  background: '#0f172a',
  white: '#a1a1aa',
  pink: '#ec4899',
  pawnNavy: '#2563eb',
  pawnRed: '#dc2626',
  pawnGreen: '#059669',
  pawnYellow: '#d97706',
}

const ANIMATION = {
  body: {
    limitX: 0.08,
    limitY: 0.2,
    smoothing: 4,
  },
  head: {
    limitX: 0.15,
    limitY: 0.2,
    smoothing: 6,
  },
  eyes: {
    radius: 0.04,
    smoothing: 8,
  },
  idle: {
    enabled: true,
    amplitude: 0.02,
    speed: 0.8,
  },
}

const mouse = { x: 0, y: 0 }
const smoothMouse = { x: 0, y: 0 }
let headStartRotation = { x: 0, y: 0, z: 0 }

const smoothDamp = (current: number, target: number, smoothing: number, deltaTime: number): number => {
  const factor = 1 - Math.exp(-smoothing * deltaTime)
  return current + (target - current) * factor
}

const initScene = () => {
  if (!container.value) return

  const width = container.value.clientWidth
  const height = container.value.clientHeight

  clock = new THREE.Clock()

  scene = new THREE.Scene()
  scene.background = new THREE.Color(COLORS.background)
  scene.fog = new THREE.Fog(COLORS.background, 12, 65)

  camera = new THREE.PerspectiveCamera(50, width / height, 0.1, 1000)
  camera.position.set(0, 2, 9)

  renderer = new THREE.WebGLRenderer({ antialias: true })
  renderer.setSize(width, height)
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
  renderer.toneMapping = THREE.ReinhardToneMapping
  renderer.toneMappingExposure = 1.4
  renderer.shadowMap.enabled = true
  container.value.appendChild(renderer.domElement)

  controls = new OrbitControls(camera, renderer.domElement)
  controls.enableDamping = true
  controls.dampingFactor = 0.05

  const ambientLight = new THREE.AmbientLight(0xffffff, 1.5)
  scene.add(ambientLight)

  const directionalLight = new THREE.DirectionalLight(0xffffff, 1.8)
  directionalLight.position.set(5, 8, 5)
  directionalLight.castShadow = true
  scene.add(directionalLight)

  const directionalLight2 = new THREE.DirectionalLight(0xffffff, 1.0)
  directionalLight2.position.set(-5, 4, -5)
  scene.add(directionalLight2)

  const pointLight1 = new THREE.PointLight(0x00e5ff, 2.0, 14)
  pointLight1.position.set(-4, 2, 4)
  scene.add(pointLight1)

  const pointLight2 = new THREE.PointLight(0xff00ff, 1.6, 12)
  pointLight2.position.set(4, 3, -2)
  scene.add(pointLight2)

  const pointLight3 = new THREE.PointLight(0xffaa00, 1.0, 10)
  pointLight3.position.set(0, -1, 3)
  scene.add(pointLight3)

  const spotLight = new THREE.SpotLight(0xffffff, 2.5)
  spotLight.position.set(0, 10, 0)
  spotLight.angle = Math.PI / 6
  spotLight.penumbra = 0.4
  spotLight.castShadow = true
  scene.add(spotLight)

  setupPostProcessing(width, height)
  loadRobot()
  createText()
  createParticles()
  createOrbitingObjects()
  createSphere()
  createRings()
}

const setupPostProcessing = (width: number, height: number) => {
  composer = new EffectComposer(renderer)
  const renderPass = new RenderPass(scene, camera)
  composer.addPass(renderPass)

  const bloomPass = new UnrealBloomPass(new THREE.Vector2(width, height), 0.6, 0.4, 0.1)
  composer.addPass(bloomPass)

  const outputPass = new OutputPass()
  composer.addPass(outputPass)
}

const loadRobot = async () => {
  const loader = new GLTFLoader()
  try {
    const gltf = await loader.loadAsync('/Robot.glb')
    robot = gltf.scene
    robot.position.set(0, 0, 0)
    robot.scale.set(1, 1, 1)

    robot.traverse((child) => {
      if ((child as THREE.Mesh).isMesh) {
        child.castShadow = true
        child.receiveShadow = true
      }
    })

    scene.add(robot)

    leftEye = robot.getObjectByName('LeftEye')!
    rightEye = robot.getObjectByName('RightEye')!
    leftPupil = robot.getObjectByName('LeftPupil')!
    rightPupil = robot.getObjectByName('RightPupil')!
    robotHead = robot.getObjectByName('Head')!

    if (robotHead) {
      headStartRotation = {
        x: robotHead.rotation.x,
        y: robotHead.rotation.y,
        z: robotHead.rotation.z,
      }
    }
  } catch (error) {
    console.error('Error loading robot:', error)
  }
}

const createSphere = () => {
  const sphereGeometry = new THREE.SphereGeometry(1.4, 24, 24)
  const sphereMaterial = new THREE.MeshBasicMaterial({
    color: COLORS.secondary,
    wireframe: true,
  })
  sphere = new THREE.Mesh(sphereGeometry, sphereMaterial)
  sphere.position.set(5.0, 2, -15)
  scene.add(sphere)
}

const createRings = () => {
  rings = new THREE.Group()

  const ringsConfig = [
    { radius: 2.2, tubeRadius: 0.04, color: COLORS.cyan },
    { radius: 2.5, tubeRadius: 0.05, color: COLORS.secondary },
  ]

  ringsConfig.forEach((config) => {
    const ringGeometry = new THREE.TorusGeometry(config.radius, config.tubeRadius, 16, 100)
    const ringMaterial = new THREE.MeshBasicMaterial({
      color: config.color,
      wireframe: true,
    })
    const ring = new THREE.Mesh(ringGeometry, ringMaterial)
    ring.rotation.x = Math.PI / 2
    rings.add(ring)
  })

  rings.position.set(5.0, 2, -15)
  rings.rotation.x = 0.2
  rings.rotation.z = 2.4
  scene.add(rings)
}

const createText = () => {
  const loader = new FontLoader()
  const textConfig = [
    { text: 'DIGITAL', position: -2.5 },
    { text: 'WARS', position: -3.75 },
  ]

  const textMaterial = new THREE.MeshBasicMaterial({ color: COLORS.secondary })

  loader.load(fontPath, (font) => {
    textGroup = new THREE.Group()

    textConfig.forEach((config) => {
      const textGeometry = new TextGeometry(config.text, {
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
      const textMesh = new THREE.Mesh(textGeometry, textMaterial)
      textMesh.position.set(centerOffset, config.position, 0)
      textGroup.add(textMesh)
    })

    textGroup.rotation.x = -0.2
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
  scene.add(particles2)
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

const createOrbitingObjects = () => {
  const objects = [
    { type: 'pawn', color: COLORS.pawnRed, orbitRadius: 2.8, orbitSpeed: 0.18, orbitTilt: 0.4, startAngle: 0 },
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

const updateRobotAnimation = (deltaTime: number, elapsed: number) => {
  if (!robot) return

  smoothMouse.x = smoothDamp(smoothMouse.x, mouse.x, 5, deltaTime)
  smoothMouse.y = smoothDamp(smoothMouse.y, mouse.y, 5, deltaTime)

  let idleX = 0
  let idleY = 0
  if (ANIMATION.idle.enabled) {
    idleX = Math.sin(elapsed * ANIMATION.idle.speed) * ANIMATION.idle.amplitude
    idleY = Math.sin(elapsed * ANIMATION.idle.speed * 0.7 + 1) * ANIMATION.idle.amplitude * 0.5
  }

  const targetBodyRotY = smoothMouse.x * ANIMATION.body.limitY + idleY
  const targetBodyRotX = smoothMouse.y * ANIMATION.body.limitX + idleX

  robot.rotation.y = smoothDamp(robot.rotation.y, targetBodyRotY, ANIMATION.body.smoothing, deltaTime)
  robot.rotation.x = smoothDamp(robot.rotation.x, targetBodyRotX, ANIMATION.body.smoothing, deltaTime)

  if (robotHead && headStartRotation) {
    const headIdleX = Math.sin(elapsed * ANIMATION.idle.speed * 1.2) * ANIMATION.idle.amplitude * 0.5
    const headIdleY = Math.cos(elapsed * ANIMATION.idle.speed * 0.9) * ANIMATION.idle.amplitude * 0.3

    const targetHeadX = headStartRotation.x - smoothMouse.y * ANIMATION.head.limitX + headIdleX
    const targetHeadY = headStartRotation.y + headIdleY

    robotHead.rotation.x = smoothDamp(robotHead.rotation.x, targetHeadX, ANIMATION.head.smoothing, deltaTime)
    robotHead.rotation.y = smoothDamp(robotHead.rotation.y, targetHeadY, ANIMATION.head.smoothing, deltaTime)
  }
}

const updateEyes = (deltaTime: number) => {
  if (!leftPupil || !rightPupil) return

  const eyeRadius = ANIMATION.eyes.radius
  let targetX = smoothMouse.x * eyeRadius
  let targetY = smoothMouse.y * eyeRadius

  const distance = Math.sqrt(targetX * targetX + targetY * targetY)
  if (distance > eyeRadius) {
    const ratio = eyeRadius / distance
    targetX *= ratio
    targetY *= ratio
  }

  leftPupil.position.x = smoothDamp(leftPupil.position.x, targetX, ANIMATION.eyes.smoothing, deltaTime)
  leftPupil.position.y = smoothDamp(leftPupil.position.y, targetY, ANIMATION.eyes.smoothing, deltaTime)
  rightPupil.position.x = smoothDamp(rightPupil.position.x, targetX, ANIMATION.eyes.smoothing, deltaTime)
  rightPupil.position.y = smoothDamp(rightPupil.position.y, targetY, ANIMATION.eyes.smoothing, deltaTime)
}

const onMouseMove = (event: MouseEvent) => {
  if (!container.value) return
  const rect = container.value.getBoundingClientRect()
  mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1
  mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1
}

const animate = () => {
  animationId = requestAnimationFrame(animate)

  const deltaTime = clock.getDelta()
  const elapsed = clock.getElapsedTime()

  const levitationSpeed = 0.4
  const levitationRange = 0.2
  const baseY = 2.0
  const baseZ = -15.0
  const levitationZRange = 0.5
  const yOffset = Math.sin(elapsed * levitationSpeed) * levitationRange
  const zOffset = Math.cos(elapsed * levitationSpeed) * levitationZRange

  if (robot) {
    robot.position.y = Math.sin(elapsed * 1.5) * 0.1
  }

  if (particles) {
    particles.rotation.y = elapsed * 0.05
    particles.position.y = Math.sin(elapsed * 0.2) * 0.2
  }

  if (particles2) {
    particles2.rotation.y = elapsed * 0.02
    particles2.position.z = Math.cos(elapsed * 0.1) * 0.2
  }

  if (orbitingObjects) {
    orbitingObjects.forEach((obj) => {
      obj.pivot.rotation.y = obj.startAngle + elapsed * obj.orbitSpeed
      obj.mesh.rotation.x += obj.selfRotationSpeed.x * 0.006
      obj.mesh.rotation.y += obj.selfRotationSpeed.y * 0.006
      obj.mesh.rotation.z += obj.selfRotationSpeed.z * 0.006
    })
  }

  if (sphere) {
    sphere.rotation.y = elapsed * 0.1
    sphere.position.y = baseY + yOffset
    sphere.position.z = baseZ + zOffset
  }

  if (rings) {
    rings.position.y = baseY + yOffset
    rings.rotation.z = baseZ + zOffset * 0.6
  }

  updateRobotAnimation(deltaTime, elapsed)
  updateEyes(deltaTime)
  controls.update()
  composer.render()
}

const handleResize = () => {
  if (!container.value) return
  const width = container.value.clientWidth
  const height = container.value.clientHeight

  camera.aspect = width / height
  camera.updateProjectionMatrix()
  renderer.setSize(width, height)
  composer.setSize(width, height)
}

onMounted(() => {
  initScene()
  animate()
  window.addEventListener('resize', handleResize)
  window.addEventListener('mousemove', onMouseMove)
})

onUnmounted(() => {
  cancelAnimationFrame(animationId)
  window.removeEventListener('resize', handleResize)
  window.removeEventListener('mousemove', onMouseMove)

  if (container.value && renderer.domElement) {
    container.value.removeChild(renderer.domElement)
  }

  renderer.dispose()
  controls.dispose()
  if (composer) composer.dispose()
})
</script>

<style scoped>
.canvas-container {
  width: 100%;
  height: 100vh;
  overflow: hidden;
  background-color: #0f172a;
  pointer-events: none;
}
</style>