import { createRouter, createWebHistory } from "vue-router"
import { useAuthStore } from "@/stores/authStore"
import { storeToRefs } from "pinia"

const HomeView = () => import('@/views/HomeView.vue')
const LoginView = () => import('@/views/LoginView.vue')

const ProjectListView = () => import('@/views/ProjectListView.vue')
const NewProjectView = () => import('@/views/NewProjectView.vue')
const ProjectView = () => import('@/views/ProjectView.vue')
const ProjectEditorView = () => import('@/views/ProjectEditorView.vue')
const ProfileView = () => import('@/views/ProfileView.vue')

const routes = [
    {
        path: "/:catchAll(.*)",
        redirect: { name: "Home" },
    },
    {
        path: "/",
        name: "Home",
        component: HomeView,
        meta: {
            title: "home_view_title",
            requireAuth: false,
        },
    },
    {
        path: "/login",
        name: "Login",
        component: LoginView,
        meta: {
            title: "login_view_title",
            requireAuth: false,
        },
    },
    {
        path: "/projects",
        name: "Projects",
        component: ProjectListView,
        meta: {
            title: "projects_view_title",
            requireAuth: true,
        },
    },
    {
        path: "/projects/new",
        name: "NewProject",
        component: NewProjectView,
        meta: {
            title: "new_project_view_title",
            requireAuth: true,
        },
    },
    {
        path: "/projects/:id",
        name: "Project",
        component: ProjectView,
        meta: {
            title: "project_view_title",
            requireAuth: true,
        },
    },
    {
        path: "/projects/:id/editor",
        name: "ProjectEditor",
        component: ProjectEditorView,
        meta: {
            title: "project_editor.view_title",
            requireAuth: true,
        },
    },
    {
        path: "/profile",
        name: "Profile",
        component: ProfileView,
        meta: {
            title: "profile_view_title",
            requireAuth: true,
        },
    }
]

const router = createRouter({
    //history: createWebHashHistory(),
    history: createWebHistory(),
    routes,
    scrollBehavior(to, from, savedPosition) {
        return savedPosition || { top: 0 };
    },
});

router.beforeEach(async (to, from) => {
    console.log(`Router::beforeEach: from: ${from.name} -> to: ${to.name}`);  

    const authStore = useAuthStore()
    const { isAuthenticated } = storeToRefs(authStore)
    console.log('Router::beforeEach: isAuthenticated=', isAuthenticated.value)

    if (isAuthenticated.value) {
        if (!to.meta.requireAuth) {
            console.log('Router::beforeEach: route does not require auth. Redirecting to Projects')
            return { name: 'Projects' }
        }
    }
    else {
        // Allow access to routes that don't require auth
        if (to.meta.requireAuth) {
            console.log('Router::beforeEach: route requires auth. Block navigation')
            return {
                name: 'Login',
                query: { redirectTo: encodeURIComponent(to.fullPath) }
            }
        }
    }
});

export default router