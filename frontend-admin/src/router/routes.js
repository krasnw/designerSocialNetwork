import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "/",
    redirect: "/login",
  },
  {
    path: "/login",
    name: "Login",
    component: () => import("../components/Login.vue"),
    meta: { requiresGuest: true },
  },
  {
    path: "/reports",
    name: "Reports",
    component: () => import("../components/reports/ReportsLayout.vue"),
    meta: { requiresAuth: true },
    children: [
      {
        path: "users",
        name: "UsersReports",
        component: () => import("../components/reports/UsersReports.vue"),
        meta: { requiresAuth: true },
      },
      {
        path: "posts",
        name: "PostsReports",
        component: () => import("../components/reports/PostsReports.vue"),
        meta: { requiresAuth: true },
      },
    ],
  },
  {
    path: "/404",
    name: "NotFound",
    component: () => import("../components/NotFound.vue"),
  },
  {
    path: "/:pathMatch(.*)*",
    redirect: "/404",
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;

router.beforeEach((to, from, next) => {
  const isAuthenticated = !!localStorage.getItem("JWT");

  if (to.meta.requiresAuth && !isAuthenticated) {
    next("/login");
  } else if (to.meta.requiresGuest && isAuthenticated) {
    next("/reports");
  } else {
    next();
  }
});
