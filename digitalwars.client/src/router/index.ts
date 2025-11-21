import { createRouter, createWebHistory } from 'vue-router'
import mainView from '@/views/landing/mainView.vue'
import adminDashboardView from '@/views/admin/adminDashboardView.vue'
import homeAdmin from '@/views/admin/homeAdminView.vue'
import statisticsView from '@/views/admin/adminStatistics.vue'
import editBoardView from '@/views/admin/editBoardView.vue'
import cheatSheetView from '@/views/admin/cheatSheetView.vue'
import adminGameDashboardView from '@/views/game/adminGameDashboardView.vue'
import playerView from '@/views/player/playerView.vue'
import gameStatistics from '@/views/game/gameStatistics.vue'
import testBoard from '@/views/testBoard.vue'
import decisionPanel from '@/views/game/decisionPanelView.vue'
import resetPasswordView from '@/views/resetPasswordView.vue'
import confirmEmailView from '@/views/confirmEmailView.vue'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import gameView from '@/views/admin/adminGameView.vue'
import tableDecisionPanelView from '@/views/game/tableDecisionPanelView.vue'
import exportToPdfView from '@/views/admin/exportToPdfView.vue'
import TableManagmentView from '@/views/admin/TableManagmentView.vue'
import GameplayElementsEditiorView from '@/views/admin/GameplayElementsEditiorView.vue'

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
          path: 'statistics',
          name: 'admin-statistics',
          component: statisticsView,
        },
        {
          path: 'editBoard',
          name: 'edit-board',
          component: editBoardView,
        },
        {
          path: 'cheatSheet',
          name: 'cheat-sheet',
          component: cheatSheetView,
        },
        {
          path: 'editGameplayElements',
          name: 'edit-gameplay-elements',
          component: GameplayElementsEditiorView,
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
      // FIX: Dodano przekierowanie, aby uniknąć pustej strony pod adresem /admin/game
      redirect: { name: 'admin-home' },
      children: [
        // FIX: Kolejność tras została zmieniona. Najbardziej szczegółowe trasy muszą być zdefiniowane jako pierwsze.
        {
          path: ':gameId/statistics',
          name: 'admin-game-statistics',
          component: gameStatistics,
          props: true,
        },
        {
          path: ':gameId/table-management',
          name: 'table-management',
          component: TableManagmentView,
          props: true,
        },
        {
          path: 'market/:gameId', // Ta trasa może być pierwsza, bo ma statyczny prefix 'market'
          name: 'decision-panel',
          component: decisionPanel,
          props: true,
        },
        {
          // Ta trasa ma dwa dynamiczne segmenty, więc musi być przed trasą z jednym segmentem.
          path: ':gameId/:teamId',
          // FIX: Poprawiono literówkę w nazwie i zapewniono unikalność
          name: 'table-decision-panel',
          component: tableDecisionPanelView,
          props: true,
        },
        {
          path: ':gameId/cheatSheet',
          name: 'admin-game-cheat-sheet',
          component: cheatSheetView,
          props: true,
        },
        {
          // Ta trasa jest najbardziej ogólna, dlatego musi być na końcu tej grupy.
          path: ':gameId',
          name: 'table-view',
          component: gameView,
          props: true,
        },
      ],
    },
    {
      path: '/player',
      redirect: '/', // Przekieruj na stronę główną, jeśli brakuje tokena
    },
    {
      path: '/player/:teamToken',
      name: 'player-dashboard',
      component: playerView,
      props: true,
    },
  ],
})

// Strażnik nawigacji jest poprawny, wprowadzono drobną poprawkę w logowaniu błędu.
router.beforeEach(async (to, from, next) => {
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)

  if (!requiresAuth) {
    return next()
  }

  try {
    await apiServices.get(apiConfig.auth.me)
    next()
  } catch (err) {
    sessionStorage.setItem('showLoginAfterRedirect', 'true')
    next('/')
    // FIX: Zmieniono na console.error dla lepszej semantyki błędu
    console.error('Błąd autoryzacji:', err)
  }
})

export default router
