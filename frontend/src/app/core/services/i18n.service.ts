import { Injectable, signal, computed } from '@angular/core';

export type Locale = 'en' | 'pt';

const translations = {
  en: {
    // Nav
    nav_home: 'Home',
    nav_movies: 'Movies',
    nav_series: 'Series',
    nav_anime: 'Anime',
    nav_premium: 'Premium Member',
    // Dashboard
    dash_welcome: 'Welcome back',
    dash_subtitle: 'Here is your entertainment overview.',
    dash_search: 'Search movies, series...',
    dash_genre_dist: 'Genre Distribution',
    dash_total: 'Total',
    dash_monthly: 'Monthly Watch Time',
    dash_recently: 'Recently Watched',
    dash_up_next: 'Up Next',
    dash_finished: 'Finished',
    dash_left: 'Left',
    dash_available: 'Available now',
    dash_ep_final: 'Final',
    // Stats
    stat_watched: 'Watched',
    stat_to_watch: 'To Watch',
    stat_hours: 'Hours Logged',
    stat_avg_rating: 'Avg. Rating',
    stat_movies: 'Movies',
    stat_series: 'Series',
    stat_anime: 'Anime',
    stat_total: 'Total in Library',
    stat_watching: 'Watching',
    stat_dropped: 'Dropped',
    // Auth
    auth_email: 'Email Address',
    auth_password: 'Password',
    auth_name: 'Full Name',
    auth_forgot: 'Forgot?',
    auth_signin: 'Sign In',
    auth_signup: 'Create Account',
    auth_or: 'Or continue with',
    auth_no_account: "Don't have an account?",
    auth_has_account: 'Already have an account?',
    auth_join: 'Join the club',
    auth_login_link: 'Sign in',
    auth_welcome: 'Welcome Back',
    auth_welcome_sub: 'Track your journey through cinema.',
    auth_register_title: 'Create Account',
    auth_register_sub: 'Start tracking your cinematic journey.',
    auth_confirm_password: 'Confirm Password',
    auth_passwords_no_match: 'Passwords do not match.',
    // Catalog
    cat_all_genres: 'All Genres',
    cat_status_all: 'Status: All',
    cat_status_watched: 'Watched',
    cat_status_towatch: 'To Watch',
    cat_status_watching: 'Watching',
    cat_status_dropped: 'Dropped',
    cat_load_more: 'Load More',
    cat_loading: 'Loading...',
    cat_empty: 'No items found.',
    cat_empty_sub: 'Try adjusting your filters.',
    cat_of: 'of',
    cat_titles: 'titles',
    // Achievements
    nav_achievements: 'Achievements',
    // Misc
    logout: 'Logout',
    theme_toggle: 'Toggle theme',
    lang_toggle: 'PT',
  },
  pt: {
    nav_home: 'Início',
    nav_movies: 'Filmes',
    nav_series: 'Séries',
    nav_anime: 'Anime',
    nav_premium: 'Membro Premium',
    dash_welcome: 'Bem-vindo de volta',
    dash_subtitle: 'Aqui está seu resumo de entretenimento.',
    dash_search: 'Buscar filmes, séries...',
    dash_genre_dist: 'Distribuição por Gênero',
    dash_total: 'Total',
    dash_monthly: 'Horas Assistidas por Mês',
    dash_recently: 'Assistidos Recentemente',
    dash_up_next: 'Próximos',
    dash_finished: 'Concluído',
    dash_left: 'Restam',
    dash_available: 'Disponível agora',
    dash_ep_final: 'Final',
    stat_watched: 'Assistidos',
    stat_to_watch: 'Para Assistir',
    stat_hours: 'Horas Registradas',
    stat_avg_rating: 'Nota Média',
    stat_movies: 'Filmes',
    stat_series: 'Séries',
    stat_anime: 'Anime',
    stat_total: 'Total na Biblioteca',
    stat_watching: 'Assistindo',
    stat_dropped: 'Abandonados',
    auth_email: 'E-mail',
    auth_password: 'Senha',
    auth_name: 'Nome Completo',
    auth_forgot: 'Esqueceu?',
    auth_signin: 'Entrar',
    auth_signup: 'Criar Conta',
    auth_or: 'Ou continue com',
    auth_no_account: 'Não tem uma conta?',
    auth_has_account: 'Já tem uma conta?',
    auth_join: 'Criar conta',
    auth_login_link: 'Entrar',
    auth_welcome: 'Bem-vindo de volta',
    auth_welcome_sub: 'Registre sua jornada pelo cinema.',
    auth_register_title: 'Criar Conta',
    auth_register_sub: 'Comece a registrar sua jornada cinematográfica.',
    auth_confirm_password: 'Confirmar Senha',
    auth_passwords_no_match: 'As senhas não coincidem.',
    cat_all_genres: 'Todos os Gêneros',
    cat_status_all: 'Status: Todos',
    cat_status_watched: 'Assistidos',
    cat_status_towatch: 'Para Assistir',
    cat_status_watching: 'Assistindo',
    cat_status_dropped: 'Abandonados',
    cat_load_more: 'Carregar Mais',
    cat_loading: 'Carregando...',
    cat_empty: 'Nenhum item encontrado.',
    cat_empty_sub: 'Tente ajustar os filtros.',
    cat_of: 'de',
    cat_titles: 'títulos',
    nav_achievements: 'Conquistas',
    logout: 'Sair',
    theme_toggle: 'Alternar tema',
    lang_toggle: 'EN',
  },
} as const;

export type TranslationKey = keyof typeof translations.en;

@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly _locale = signal<Locale>(
    (localStorage.getItem('locale') as Locale) ?? 'en'
  );

  readonly locale = this._locale.asReadonly();

  readonly t = computed(() => translations[this._locale()]);

  translate(key: TranslationKey): string {
    return translations[this._locale()][key];
  }

  toggleLocale() {
    const next: Locale = this._locale() === 'en' ? 'pt' : 'en';
    this._locale.set(next);
    localStorage.setItem('locale', next);
  }

  setLocale(locale: Locale) {
    this._locale.set(locale);
    localStorage.setItem('locale', locale);
  }
}
