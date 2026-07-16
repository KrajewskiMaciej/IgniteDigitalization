import { createRouter, createWebHistory } from 'vue-router'
import mainView from '@/views/landing/mainView.vue'
import adminDashboardView from '@/views/admin/adminDashboardView.vue'
import homeAdmin from '@/views/admin/homeAdminView.vue'
import editBoardView from '@/views/admin/editBoardView.vue'
import adminGameDashboardView from '@/views/game/adminGameDashboardView.vue'
import playerView from '@/views/player/playerView.vue'
import testBoard from '@/views/testBoard.vue'
import tableDecisionPanelView from '@/views/game/tableDecisionPanelView.vue'
import resetPasswordView from '@/views/resetPasswordView.vue'
import confirmEmailView from '@/views/confirmEmailView.vue'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import { useAuthStore } from '@/stores/auth'
import gameView from '@/views/admin/adminGameView.vue'
import exportToPdfView from '@/views/admin/exportToPdfView.vue'
import TableManagmentView from '@/views/admin/TableManagmentView.vue'
import GameplayElementsEditiorView from '@/views/admin/GameplayElementsEditiorView.vue'
import DynamicCheatSheetView from '@/views/admin/DynamicCheatSheetView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'main',
      component: mainView,
    },
    {
      path: '/testBoard',
      name: 'test-board',
      component: testBoard,
    },
    {
      path: '/resetPassword/:token',
      name: 'reset-password',
      component: resetPasswordView,
    },
    {
      path: '/confirm/:token',
      name: 'confirm-email',
      component: confirmEmailView,
    },
    {
      path: '/admin',
      name: 'admin-dashboard',
      component: adminDashboardView,
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'admin-home',
          component: homeAdmin,
        },
        {
          path: 'editBoard',
          name: 'edit-board',
          component: editBoardView,
          meta: { requiresRole: 9 },
        },
        {
          path: 'editGameplayElements',
          name: 'edit-gameplay-elements',
          component: GameplayElementsEditiorView,
          meta: { requiresRole: 9 },
        },
        {
          path: 'exportToPDF',
          name: 'export-PDF',
          component: exportToPdfView,
        },
      ],
    },
    {
      path: '/admin/game',
      component: adminGameDashboardView,
      meta: { requiresAuth: true },
      redirect: { name: 'admin-home' },
      children: [
        {
          path: ':gameId/table-management',
          name: 'table-management',
          component: TableManagmentView,
          props: true,
        },
        {
          path: 'market/:gameId',
          name: 'decision-panel',
          component: tableDecisionPanelView,
          props: (route) => ({ gameId: route.params.gameId, allTeams: true }),
        },
        {
          path: ':gameId/:teamId',
          name: 'table-decision-panel',
          component: tableDecisionPanelView,
          props: true,
        },
        {
          path: ':gameId',
          name: 'table-view',
          component: gameView,
          props: true,
        },
        {
          path: ':gameId/DynamicCheatSheet',
          name: 'dynamic-cheat-sheet',
          component: DynamicCheatSheetView,
          props: true,
        },
      ],
    },
    {
      path: '/player',
      redirect: '/',
    },
    {
      path: '/player/:teamToken',
      name: 'player-dashboard',
      component: playerView,
      props: true,
    },
  ],
})

router.beforeEach(async (to, from, next) => {
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)

  if (!requiresAuth) {
    return next()
  }

  try {
    const res = await apiServices.get<{ role: number }>(apiConfig.auth.me)
    const auth = useAuthStore()
    auth.setAuthenticated(true)
    auth.role = res.data.role

    const required = to.matched.find((r) => r.meta.requiresRole)?.meta.requiresRole as
      | number
      | undefined
    if (required && res.data.role < required) {
      return next({ name: 'admin-home' })
    }
    next()
  } catch (err) {
    sessionStorage.setItem('showLoginAfterRedirect', 'true')
    next('/')
    console.error('Błąd autoryzacji:', err)
  }
})

export default router
